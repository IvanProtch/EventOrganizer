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
        public Organizer(IUser user)
        {
            _user = user;
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
                    _events = new Events(_dbUser);

                    _actualEvents = db.Events.Where(e => e.UserId == _dbUser.Id);
                }
                else
                {
                    throw new Exception($"Не удалось получить пользователя по login = {_user.Login}");
                }

            }
        }

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

        private Events _events;
        public Events Events { get => _events; }



    }

}