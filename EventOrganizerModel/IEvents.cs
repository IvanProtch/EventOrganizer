using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventOrganizerModel
{
    public class Events
    {
        protected User _user;
        internal Events(User dbUser)
        {
            _user = dbUser;

        }

        private Queue<Event> _upcomingEvents;

        internal void InvokeEventHappened(Event e) => EventHappened.Invoke(this, e);

        public event EventHappenedHandler EventHappened;

        public void Add(Event newEvent)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                db.Events.Add(newEvent);
                if (newEvent.IsUpcoming || newEvent.IsRunning)
                {
                    _upcomingEvents.Prepend(newEvent);
                    _upcomingEvents.OrderBy(e => e.Start);
                }
                db.SaveChanges();
                
            }
        }

        private IEnumerable<Event> GetUserEvents()
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                if (_user != null)
                {
                    return db.Events.Where(e => e.UserId == _user.Id);
                }
                throw new Exception($"Не найдены события для {_user.Login}");
            }
        }

        public List<Event> GetEvents(DateTime from, DateTime to) => 
            GetUserEvents()
                .Where(e => from <= DateTime.Parse(e.Start.ToString()) && to >= DateTime.Parse(e.End.ToString()))
                .ToList();

        public List<Event> GetEvents(int count) => GetUserEvents().Take(count).ToList();

        public List<Event> GetEventsForMonth(int monthNo) => GetUserEvents().Where(e => e.Start.Month == monthNo).ToList();

        private void EventsLoop()
        {
            var toHistory = new List<Event>();
            while (EventsPool.Events.Count > 0)
            {
                foreach (var eventItem in EventsPool.Events)
                {
                    if (eventItem.IsRunning)
                    {
                        _eventsPool.InvokeEventHappened(eventItem);
                        toHistory.Add(eventItem);
                        if (eventItem.RepeatsTimeSpan.TotalMinutes != 0)
                        {
                            var newEvent = eventItem.Clone() as Event;
                            _dbUser.Events.Add(newEvent);
                            _dbContext.SaveChanges();
                        }
                    }
                }
                if (toHistory.Count > 0)
                {
                    _eventsPool.Events = _eventsPool.Events.Except(toHistory).ToList();
                    _dbUser.Events.AddRange(toHistory);
                    _dbContext.SaveChanges();
                    toHistory = new List<Event>();
                }
                Task.Delay(1000);
            }

        }
    }

    public delegate void EventHappenedHandler(object sender, Event e);

}