using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

public class Event : ICloneable
{
    public Event() { }

    public Event(string caption, string description, string eventTypeName, DateTime start, DateTime end, TimeSpan repeatSpan)
    {
        this.Caption = caption;
        this.Description = description;
        this.Start = start;
        this.End = end;
        this.Repeats = repeatSpan;
        this.EventTypeName = eventTypeName;

        GetEventTypeByName(EventTypeName);
        if (Start > End)
            throw new Exception($"Неверно указаны даты: начало = {start}, конец = {End}");
    }


    public int Id { get; }
    
    public string EventTypeName { get; set; }
    public int UserId { get; set; }
    public string Caption { get; set; }
    public string Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public EventType EventType { get; set; }

    public TimeSpan Repeats { get; set; }

    public bool IsRunning { get => DateTime.Now <= End && DateTime.Now >= Start; }
    public bool IsComplete { get => DateTime.Now >= End; }
    public bool IsUpcoming { get => Start <= DateTime.Now; }

    public void ConfirmChanges()
    {
        using (ApplicationDBContext db = new ApplicationDBContext())
        {
            db.Events.Update(this);
            db.SaveChanges();
        }
    }

    private void GetEventTypeByName(string name)
    {
        using (ApplicationDBContext db = new ApplicationDBContext())
        {
            var bdEventType = db.EventTypes.Find(EventTypeName);
            if(bdEventType == null)
            {
                EventType = bdEventType;
            }
            else
            {
                throw new Exception($"Не найден тип события {name}");
            }
        }

    }

    public object Clone()
    {
        return new Event()
        {
            Caption = this.Caption,
            Description = this.Description,
            Start = this.Start + this.Repeats,
            End = this.End + this.Repeats,
            EventType = this.EventType,
        };
    }
}

public class EventType
{

    //public int Id { get; set; }
    [Key]
    public string TypeName { get; set; }
    public int ColorRGB { get; set; }

    [NotMapped]
    public Color Color
    {
        get => System.Drawing.Color.FromArgb(ColorRGB);
        set
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                var type = db.EventTypes.Find(TypeName);
                if(type != null)
                {
                    type.ColorRGB = value.ToArgb();
                    db.EventTypes.Update(type);
                    db.SaveChanges();
                }
            }
        } 
    }

}
