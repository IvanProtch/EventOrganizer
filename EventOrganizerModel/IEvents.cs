using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventOrganizerModel
{
    public class EventsHistory
    {
        protected User _user;
        internal EventsHistory(User dbUser) => _user = dbUser;

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

    }

    public delegate void EventHappenedHandler(object sender, Event e);

    public class EventsPool : EventsHistory
    {
        public List<Event> Events { get; set; } = new List<Event>();

        internal EventsPool(User dbUser) : base(dbUser) => _user = dbUser;

        internal void InvokeEventHappened(Event e) => EventHappened.Invoke(this, e);

        public event EventHappenedHandler EventHappened;
    }

}