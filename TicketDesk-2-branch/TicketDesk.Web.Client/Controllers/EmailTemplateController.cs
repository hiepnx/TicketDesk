﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TicketDesk.Web.Client.Models;
using System.IO;
using System.ComponentModel.Composition;
using TicketDesk.Domain.Services;
using System.Text;
using System.Web.Mvc.Html;
using System.Web.Routing;
using TicketDesk.Domain.Models;
using System.Configuration;

namespace TicketDesk.Web.Client.Controllers
{
    [HandleError]
    [Export("EmailTemplate", typeof(IController))]
    public partial class EmailTemplateController : ApplicationController
    {

        public EmailTemplateController() 
        {
            var sec = MefHttpApplication.ApplicationContainer.GetExportedValue<ISecurityService>();
            
            base.Security = sec;
        }

        #region temporary stuff needed only during development (or needs to go to unit testing)
        
        [Import(typeof(INotificationSendingService))]
        private TicketDesk.Domain.Services.NotificationSendingService noteService { get; set; }

        public string Test()
        {
            noteService.ProcessWaitingTicketEventNotifications();
            return "Notes have been processed";
        }

        public virtual ViewResult DisplayHtml()
        {
            this.Security.GetCurrentUserName = delegate() { return "toastman"; };
            
            var ticketService = new TicketService(Security,new TicketDesk.Domain.Repositories.TicketRepository(), null);
            var ticket = ticketService.GetTicket(82);
            var note = ticket.TicketComments.SingleOrDefault(tc => tc.CommentId == 698).TicketEventNotifications.SingleOrDefault(tn => tn.NotifyUser == "toastman");
            ViewData.Add("siteRootUrl", ConfigurationManager.AppSettings["SiteRootUrlForEmail"]);
            ViewData.Add("firstUnsentCommentId", 697);
            ViewData.Add("formatForEmail", true);
         
            
            return View(MVC.EmailTemplate.Views.TicketNotificationHtmlEmailTemplate, note );
        }

        #endregion


        public string GenerateTicketNotificationTextEmailBody(TicketEventNotification notification, int firstUnsentCommentId)
        {
            var templateName = "~/Views/EmailTemplate/TicketNotificationTextEmailTemplate.ascx";
            return GenerateTicketNotificationEmailBody(notification, firstUnsentCommentId, templateName);
        }

        public string GenerateTicketNotificationHtmlEmailBody(TicketEventNotification notification, int firstUnsentCommentId)
        {
            var templateToRender = "~/Views/EmailTemplate/TicketNotificationHtmlEmailTemplate.ascx";
            return GenerateTicketNotificationEmailBody(notification, firstUnsentCommentId, templateToRender);
        }


        private string GenerateTicketNotificationEmailBody(TicketEventNotification notification, int firstUnsentCommentId, string templateToRender)
        {
            this.Security.GetCurrentUserName = delegate() { return notification.NotifyUser; };
            var ticket = notification.TicketComment.Ticket;
           
            var vd = new ViewDataDictionary(notification);
            vd.Add("siteRootUrl", ConfigurationManager.AppSettings["SiteRootUrlForEmail"]);
            vd.Add("firstUnsentCommentId", firstUnsentCommentId);
            vd.Add("formatForEmail", true);
            using (StringWriter sw = new StringWriter())
            {
                var fakeResponse = new HttpResponse(sw);
                var fakeContext = new HttpContext(new HttpRequest("", "http://anywherefake.com/", ""), fakeResponse);
                var fakeControllerContext = new ControllerContext
                (
                    new HttpContextWrapper(fakeContext),
                    new RouteData(),
                    this
                );
                fakeControllerContext.RouteData.Values.Add("controller", "EmailTemplate");
                
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(fakeControllerContext, templateToRender);
                ViewContext vc = new ViewContext(fakeControllerContext, new FakeView(), vd, new TempDataDictionary(), sw);

                HtmlHelper h = new HtmlHelper(vc, new ViewPage());

                h.RenderPartial(templateToRender, notification, vd);
                
                return sw.GetStringBuilder().ToString();
            }
        }

        public class FakeView : IView
        {
            #region IView Members
            public void Render(ViewContext viewContext, System.IO.TextWriter writer)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }
}