using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventOrganizer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventOrganizerController : ControllerBase
    {
        private readonly ILogger<EventOrganizerController> _logger;
        private OrganizerService service;
        public EventOrganizerController(ILogger<EventOrganizerController> logger)
        {
            _logger = logger;
            service = new OrganizerService();

        }

        [HttpGet("For today {day, month, year}")]
        public IEnumerable<Event> GetEventsForDay(int day, int month, int year) => service.Events.GetEventsForDay(day, month, year);

        [HttpGet("Range {startDate, endDate}")]
        public IEnumerable<Event> GetEventsRange(string startDate, string endDate) => service.Events.GetEvents(DateTime.Parse(startDate), DateTime.Parse(endDate));

        [HttpGet("All {count}")]
        public IEnumerable<Event> GetEventsCount(int count) => service.Events.GetEvents(count);

        [HttpGet("Upcoming {count}")]
        public IEnumerable<Event> GetUpcomingEvents(int count) => service.Events.GetUpcomingEvents(count);

        [HttpGet("Past {count}")]
        public IEnumerable<Event> GetTodayEvents(int count) => service.Events.GetPastEvents(count);

        [HttpPost("{newEvent}")]
        public void Add(Event newEvent) => service.Events.Add(newEvent);

        [HttpPut("{newEvent}")]
        public void Put(Event newEvent)
        {
            if (service.Events.GetEventOrDefaultById(newEvent.Id) != null)
                newEvent.ConfirmChanges();
        }

        [HttpPut("{deletedEvent}")]
        public void Delete(Event deletedEvent)
        {
            if (service.Events.GetEventOrDefaultById(deletedEvent.Id) != null)
                service.Events.Remove(deletedEvent);
        }
    }
}
