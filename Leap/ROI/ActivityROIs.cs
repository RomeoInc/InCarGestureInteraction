using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leap.ROI
{
    class ActivityROIs
    {
        public static void ConnectROIs(GestureDetector gestureDetector)
        {
            // Activity: MainActivity
            List<ROI> mainROIs = new List<ROI>();
            mainROIs.Add(ROIData.CreateROI("Music", 1, 1, "", 1, 1));
            mainROIs.Add(ROIData.CreateROI("GPS", 2, 1, "", 1, 2));
            mainROIs.Add(ROIData.CreateROI("Contacts", 3, 1, "Inbox", 2, 1));
            mainROIs.Add(ROIData.CreateROI("Extras", 4, 1, "", 2, 2));

            ROISet main = new ROISet(mainROIs, "StartMenu", true);
            gestureDetector.RegisterROISet(main);

            // Activity: InboxActivity
            List<ROI> inboxROIs = new List<ROI>();
            inboxROIs.Add(ROIData.CreateButtonListROI("Message1", 1, 0, "InboxItem", 1));
            inboxROIs.Add(ROIData.CreateButtonListROI("Message2", 2, 0, "InboxItem", 2));
            inboxROIs.Add(ROIData.CreateButtonListROI("Message3", 3, 0, "InboxItem", 3));
            inboxROIs.Add(ROIData.CreateButtonListROI("Message4", 4, 0, "InboxItem", 4));
            inboxROIs.Add(ROIData.CreateButtonListROI("Message5", 5, 0, "InboxItem", 5));
            inboxROIs.Add(ROIData.CreateHeaderButtonROI("Back", 0, 0, "Main"));

            ROISet inbox = new ROISet(inboxROIs, "Inbox", false);
            gestureDetector.RegisterROISet(inbox);

            // Activity: InboxItemActivity
            List<ROI> inboxItemROIs = new List<ROI>();
            inboxItemROIs.Add(ROIData.CreateDetailButtonROI("Button1", 1, 0, "", 1));
            inboxItemROIs.Add(ROIData.CreateDetailButtonROI("Button2", 2, 0, "", 2));
            inboxItemROIs.Add(ROIData.CreateDetailButtonROI("Button3", 3, 0, "", 3));
            inboxItemROIs.Add(ROIData.CreateDetailButtonROI("Button4", 4, 0, "", 4));
            inboxItemROIs.Add(ROIData.CreateHeaderButtonROI("Back", 0, 0, "Inbox"));

            ROISet inboxItem = new ROISet(inboxItemROIs, "InboxItem", false);
            gestureDetector.RegisterROISet(inboxItem);

            // Activity: ContactsActivity
            List<ROI> contactROIs = new List<ROI>();
            contactROIs.Add(ROIData.CreateButtonListROI("Contact1", 1, 0, "ContactsItem", 1));
            contactROIs.Add(ROIData.CreateButtonListROI("Contact2", 2, 0, "ContactsItem", 2));
            contactROIs.Add(ROIData.CreateButtonListROI("Contact3", 3, 0, "ContactsItem", 3));
            contactROIs.Add(ROIData.CreateButtonListROI("Contact4", 4, 0, "ContactsItem", 4));
            contactROIs.Add(ROIData.CreateButtonListROI("Contact5", 5, 0, "ContactsItem", 5));
            contactROIs.Add(ROIData.CreateHeaderButtonROI("Back", 0, 0, "Main"));

            ROISet contacts = new ROISet(contactROIs, "Contacts", false);
            gestureDetector.RegisterROISet(contacts);

            // Activity: ContactsItemActivity
            List<ROI> contactItemROIs = new List<ROI>();
            contactItemROIs.Add(ROIData.CreateDetailButtonROI("Button1", 1, 0, "", 1));
            contactItemROIs.Add(ROIData.CreateDetailButtonROI("Button2", 2, 0, "", 2));
            contactItemROIs.Add(ROIData.CreateDetailButtonROI("Button3", 3, 0, "", 3));
            contactItemROIs.Add(ROIData.CreateDetailButtonROI("Button4", 4, 0, "", 4));
            contactItemROIs.Add(ROIData.CreateHeaderButtonROI("Back", 0, 0, "Contacts"));

            ROISet contactsItem = new ROISet(contactItemROIs, "ContactsItem", false);
            gestureDetector.RegisterROISet(contactsItem);
        }
    }
}
