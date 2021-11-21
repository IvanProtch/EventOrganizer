using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventOrganizerModel
{
    public class EventsHistory
    {
        protected User _user;
        internal EventsHistory(User dbUser)
        {
            _user = dbUser;
        }
        public List<Event> Events { get; set; } = new List<Event>();

        public List<Event> GetEvents(DateTime from, DateTime to)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                if (_user != null)
                {
                    return db.Events.Where(e => from <= DateTime.Parse(e.Start.ToString()) && to >= DateTime.Parse(e.End.ToString())).ToList();
                }
                return new List<Event>();
            }
        }

        public List<Event> GetEvents(int count)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                if (_user != null)
                {
                    return db.Events.Take(count).ToList();
                }
                return new List<Event>();
            }
        }
    }

    public delegate void EventHappenedHandler(object sender, Event e);

    public class EventsPool : EventsHistory
    {
        internal EventsPool(User dbUser) : base(dbUser)
        {

        }
        internal void InvokeEventHappened(Event e)
        {
            EventHappened.Invoke(this, e);
        }
        public event EventHappenedHandler EventHappened;
    }

}