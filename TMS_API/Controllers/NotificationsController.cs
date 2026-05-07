using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_SharedLibrary.Models;

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepo _repo;

        public NotificationsController(INotificationRepo repo)
        {
            _repo = repo;
        }

        // GET: api/Notifications
        [HttpGet]
        public ActionResult<IEnumerable<Notification>> Get()
        {
            var notifications = _repo.GetAll();
            return Ok(notifications);
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public ActionResult<Notification> Get(int id)
        {
            var notification = _repo.FindById(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }

        // GET: api/Notifications/ByRecipient/5
        [HttpGet("ByRecipient/{recipientId}")]
        public ActionResult<IEnumerable<Notification>> GetByRecipient(int recipientId)
        {
            var notifications = _repo.GetByRecipientId(recipientId);
            return Ok(notifications);
        }

        // POST: api/Notifications
        [HttpPost]
        public ActionResult<Notification> Post([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest("Notification data is required");
            }

            notification.DateSent = DateTime.Now;
            var createdNotification = _repo.Create(notification);
            
            return CreatedAtAction(nameof(Get), new { id = createdNotification.NotificationId }, createdNotification);
        }

        // PUT: api/Notifications/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Notification notification)
        {
            if (notification == null || id != notification.NotificationId)
            {
                return BadRequest("Invalid notification data");
            }

            var existingNotification = _repo.FindById(id);
            if (existingNotification == null)
            {
                return NotFound();
            }

            _repo.Update(notification);
            return NoContent();
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var notification = _repo.FindById(id);
            if (notification == null)
            {
                return NotFound();
            }

            _repo.Delete(id);
            return NoContent();
        }
    }
}
