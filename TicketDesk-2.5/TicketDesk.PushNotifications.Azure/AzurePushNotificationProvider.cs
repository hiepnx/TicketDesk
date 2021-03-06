﻿// TicketDesk - Attribution notice
// Contributor(s):
//
//      Stephen Redd (stephen@reddnet.net, http://www.reddnet.net)
//
// This file is distributed under the terms of the Microsoft Public 
// License (Ms-PL). See http://opensource.org/licenses/MS-PL
// for the complete terms of use. 
//
// For any distribution that contains code from this file, this notice of 
// attribution must remain intact, and a copy of the license must be 
// provided to the recipient.

using System.Collections.Generic;
using System.Threading.Tasks;
using TicketDesk.IO;
using TicketDesk.PushNotifications.Common;
using TicketDesk.PushNotifications.Common.Model;

namespace TicketDesk.PushNotifications.Azure
{
    public class AzurePushNotificationProvider: IPushNotificationProvider
    {
        private const string QueueName = "ticket-notifications-queue";

        private AzureQueueProvider Queue { get; set; }
        public AzurePushNotificationProvider()
        {
            Queue = new AzureQueueProvider(QueueName);
        }

        /// <summary>
        /// Gets a value indicating whether this provider is correctly configured and available for use.
        /// </summary>
        /// <value><c>true</c> if this provider is available; otherwise, <c>false</c>.</value>
        public bool IsConfigured
        {
            get
            {
                return Queue != null && Queue.IsConfigured;
            }
        }

        public async Task<bool> SendNotification(IEnumerable<PushNotificationItem> items)
        {
            //await Queue.EnqueueItemsAsync(items);
            return true;
        }
    }
}
