using System;
using System.Drawing;

namespace EventOrganizerModel
{
    public class Event : ICloneable
    {
        public int EventTypeId { get; set; }
        public int UserId { get; set; }

        public int Id { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public EventType EventType { get; set; }
        public TimeSpan RepeatsTimeSpan { get; set; } = new TimeSpan();

        public bool IsRunning
        {
            get
            {
                return DateTime.Now <= End && DateTime.Now >= Start;
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }

    public class EventType
    {
        public enum Types
        {
            Meeting,
            Homework,
            Reminder
        }

        //public EventType(Types type, Color color)
        //{
        //    this.Type = type;
        //    this.Color = color;
        //}
        public int Id { get; set; }
        public Types Type { get; set; }
        //public Color Color { get; set; }

    }

}