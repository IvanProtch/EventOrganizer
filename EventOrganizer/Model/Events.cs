using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

    public class Events
    {
        protected User _user;
        private bool _eventLoopIsRunning;
        private ApplicationDBContext _db = new ApplicationDBContext();
        internal Events(User dbUser)
        {
            _user = dbUser;

            var dictionary = GetUserEvents()
                    .Where(e => e.Start <= DateTime.Now)
                    .ToDictionary(k => k.Start, v => v);

            //предстоящие события
            _upcomingEvents = dictionary == null ? new SortedDictionary<DateTime, Event>() : new SortedDictionary<DateTime, Event>(dictionary);

            if (_upcomingEvents.Count > 0)
                Task.Run(InvokeEvent);
        }
        ~Events()
        {
            _db.Dispose();
        }
        private SortedDictionary<DateTime, Event> _upcomingEvents;

        /// <summary>
        /// Получить событие, выполняемое в выбранную дату
        /// </summary>
        /// <param name="startEndIntervalDate"></param>
        /// <returns></returns>
        public Event this[DateTime startEndIntervalDate] 
        {
            get
            {
                Event e = GetUserEvents().FirstOrDefault(e => startEndIntervalDate >= e.Start && startEndIntervalDate <= e.End);
                if (e == null)
                    throw new Exception($"Не удалось найти событие по дате {startEndIntervalDate}");
                else
                    return e;
            }
        }

        public event EventHappenedHandler EventHappened;

        public void Add(Event newEvent)
        {
            _db.Events.Add(newEvent);
            if (newEvent.IsUpcoming || newEvent.IsRunning)
            {
                if (_upcomingEvents.TryAdd(newEvent.Start, newEvent))
                    if (!_eventLoopIsRunning) Task.Run(InvokeEvent);
            }
            _db.SaveChanges();
        }

        public void Remove(Event removableEvent)
        {
            _db.Events.Remove(removableEvent);
            if (removableEvent.IsUpcoming || removableEvent.IsRunning)
            {
                if (_upcomingEvents.ContainsKey(removableEvent.Start))
                {
                    _upcomingEvents.Remove(removableEvent.Start);
                }
            }
            _db.SaveChanges();
        }

        private IQueryable<Event> GetUserEvents()
        {
            if (_user != null)
                return _db.Events.Where(e => e.UserId == _user.Id);
            else
                throw new Exception($"Не найдены события {_user.Login}");
        }

        public IEnumerable<Event> GetUpcomingEvents(int count) => _upcomingEvents.Values.Take(count);

        public IEnumerable<Event> GetPastEvents(int count, bool orderByDescending = false) => GetOrderedEvents(orderByDescending).Where(e => e.IsComplete).Take(count);

        public IEnumerable<Event> GetEvents(DateTime from, DateTime to) => 
            GetOrderedEvents()
                .Where(e => from <= e.Start && to >= e.End);

        public IEnumerable<Event> GetEvents(int count, bool orderByDescending = false) => GetOrderedEvents(orderByDescending).Take(count);

        private IQueryable<Event> GetOrderedEvents(bool orderByDescending = false)
        {
            var ordered = GetUserEvents();
            if (!orderByDescending)
                ordered = ordered.OrderBy(e => e.Start);
            else
                ordered = ordered.OrderByDescending(e => e.Start);

            return ordered;
        }

        public IEnumerable<Event> GetEventsForMonth(int monthNo, int year) => GetUserEvents().Where(e =>e.Start.Year == year && e.Start.Month == monthNo);

        public IEnumerable<Event> GetEventsForDay(int day, int monthNo, int year) => 
            GetUserEvents().Where(e => e.Start.Year == year && e.Start.Month == monthNo && e.Start.Day == day);

        public IEnumerable<Event> GetTodayEvents() => GetUserEvents().Where(e => e.Start.Date == DateTime.Now.Date);

        public Event GetEventOrDefaultById(int id) => GetUserEvents().FirstOrDefault(e => e.Id == id);

        private void InvokeEvent()
        {
            var upcomingEvent = _upcomingEvents.Values.FirstOrDefault();
            if (upcomingEvent != null)
            {
                _eventLoopIsRunning = true;
                TimeSpan waitingTime = upcomingEvent.Start - DateTime.Now;
                Timer timer = new Timer(callback =>
                {
                    //если в период ожидания событие было исключено из _upcomingEvents
                    if (upcomingEvent == null)
                        return;

                    if (upcomingEvent.IsRunning)
                    {
                        _upcomingEvents.Remove(upcomingEvent.Start);

                        //для повторяемых событий
                        if (upcomingEvent.Repeats.TotalSeconds != 0)
                        {
                            Event newUpcomingEvent = upcomingEvent.Clone() as Event;
                            this.Add(newUpcomingEvent);
                        }
                        EventHappened.Invoke(this, upcomingEvent);
                    }
                    if (_upcomingEvents.Count > 0)
                        InvokeEvent();
                }, upcomingEvent, (int)Math.Abs(waitingTime.TotalMilliseconds), Timeout.Infinite);
                
            }
            _eventLoopIsRunning = false;
        }
    }
    public delegate void EventHappenedHandler(object sender, Event e);