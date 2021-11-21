using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventOrganizerModel
{
    public class Organizer
    {
        private IUser _user;
        private ApplicationDBContext _dbContext;
        private User _dbUser;
        private IQueryable<Event> _actualEvents;
        public Organizer(IUser user, ref DateTime selectedDateTime)
        {
            _user = user;
            _selectedDateTime = selectedDateTime;
            Init();
        }

        private void Init()
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                //User user1 = new User { FullName = "Петров Иван Иванович", Login = "pii" };
                //User user2 = new User { FullName = "Иванов Петр Петрович", Login = "ipp" };

                //Event event1 = new Event { Caption = "Заправить кровать", Description = "Заправление кровати", Start = new DateTime(2021, 11, 21, 10, 0, 0), End = new DateTime(2021, 11, 21, 10, 5, 0), EventType = new EventType { Type = EventType.Types.Homework} };
                //Event event2 = new Event { Caption = "Готовка", Description = "Готовка", Start = new DateTime(2021, 11, 21, 10, 0, 0), End = new DateTime(2021, 11, 21, 10, 5, 0), EventType = new EventType { Type = EventType.Types.Homework } };

                //user1.Events.Add(event1);
                //user1.Events.Add(event2);

                //user2.Events.Add(event1);
                //user2.Events.Add(event2);

                //db.Users.Add(user1);
                //db.Users.Add(user2);

                //db.SaveChanges();
                _dbContext = db;
                _dbUser = db.Users.FirstOrDefault(e => e.Login == _user.Login);

                if(_dbUser != null)
                {
                    _eventsHistory = new EventsHistory(_dbUser);
                    _eventsPool = new EventsPool(_dbUser);

                    _actualEvents = db.Events.Where(e => e.UserId == _dbUser.Id);

                    _previousSelectedDateTime = _selectedDateTime;

                    Task eventLoopTask = new Task(EventsLoop);
                    eventLoopTask.Start();
                }
                else
                {
                    throw new Exception($"Не удалось получить пользователя по login = {_user.Login}");
                }

            }
        }

        private DateTime _previousSelectedDateTime;

        private DateTime _selectedDateTime;

        private EventsPool _eventsPool;
        public EventsPool EventsPool
        {
            get 
            {
                if(_eventsPool.Events.Count == 0)
                    _eventsPool.Events = _actualEvents.Where(e => (e.Start.Date == DateTime.Now.Date))
                        .Where(e => (e.Start.Hour == DateTime.Now.Hour)).ToList();

                return _eventsPool;
            }
        }

        private EventsHistory _eventsHistory;
        public EventsHistory EventsHistory
        {
            get
            {
                if (_previousSelectedDateTime.Month != _selectedDateTime.Month)
                    _eventsHistory.Events = _actualEvents.Where(e => e.Start.Month == _selectedDateTime.Month).ToList();

                return _eventsHistory;
            }
        }

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
                        if(eventItem.RepeatsTimeSpan.TotalMinutes != 0)
                        {
                            var newEvent = eventItem.Clone() as Event;
                            newEvent.Start += newEvent.RepeatsTimeSpan;
                            newEvent.End += newEvent.RepeatsTimeSpan;

                            _dbUser.Events.Add(newEvent);
                            _dbContext.SaveChanges();
                        }
                    }
                }
                if(toHistory.Count > 0)
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

}