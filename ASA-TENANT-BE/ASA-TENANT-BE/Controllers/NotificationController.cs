﻿using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetFiltered([FromQuery] NotificationGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _notificationService.GetFilteredCategoriesAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<NotificationResponse>> Create([FromBody] NotificationRequest request)
        {
            var result = await _notificationService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<NotificationResponse>> Update(long id, [FromBody] NotificationRequest request)
        {
            var result = await _notificationService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _notificationService.DeleteAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(long id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(result);
        }

        [HttpPut("users/{userId}/read-all")]
        public async Task<ActionResult<ApiResponse<int>>> MarkAllAsReadByUser(long userId)
        {
            var result = await _notificationService.MarkAllAsReadByUserAsync(userId);
            return Ok(result);
        }

        [HttpPost("broadcast")]
        public async Task<ActionResult<ApiResponse<bool>>> BroadcastToAllShops([FromBody] BroadcastNotificationRequest request)
        {
            try
            {
                var result = await _notificationService.BroadcastToAllShopsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
