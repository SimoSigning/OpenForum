using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Messages;

namespace OpenForum.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IMessageService messageService;

        public MessageController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            InboxView model = await messageService.GetInboxAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Sent()
        {
            SentMessagesView model = await messageService.GetSentAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                MessageDetailsView model = await messageService.GetDetailsAsync(id);

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Compose(string? recipientUserName, string? returnUrl)
        {
            try
            {
                ComposeMessageView model;

                if (!string.IsNullOrWhiteSpace(recipientUserName))
                {
                    model = await messageService.GetComposeViewAsync(recipientUserName);
                    model.IsRecipientPreselected = true;
                }
                else
                {
                    model = new ComposeMessageView();
                }

                model.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Inbox));

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int originalMessageId)
        {
            try
            {
                ComposeMessageView model =await messageService.GetReplyComposeViewAsync(originalMessageId);

                model.ReturnUrl = Url.Action(nameof(Details), new { id = originalMessageId });

                return View("Compose", model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(ComposeMessageView model)
        {

            model.ReturnUrl = Url.IsLocalUrl(model.ReturnUrl)? model.ReturnUrl : Url.Action(nameof(Inbox));

            // For replies, the service generates the subject from the
            // original message. It does not rely on the submitted subject.
            if (model.IsReply)
            {
                ModelState.Remove(nameof(model.Subject));
            }

            if (!ModelState.IsValid)
            {
                return View("Compose", model);
            }

            try
            {
                await messageService.SendAsync(model);

                TempData["Success"] = "Message sent successfully.";

                return RedirectToAction(nameof(Inbox));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View("Compose", model);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View("Compose", model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View("Compose", model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSent(int id)
        {
            try
            {
                await messageService.DeleteSentMessageAsync(id);

                TempData["Success"] = "Message deleted from sent messages.";

                return RedirectToAction(nameof(Sent));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInbox(int id)
        {
            try
            {
                await messageService.DeleteInboxMessageAsync(id);

                TempData["Success"] = "Message removed from your inbox.";

                return RedirectToAction(nameof(Inbox));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

    }
}