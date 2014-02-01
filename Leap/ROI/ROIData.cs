using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Leap.ROI
{
    /// <summary>
    /// This class provides functions for generating Regions Of Interest
    /// in the Leap gesture workspace.
    /// </summary>
    class ROIData
    {
        #region Constants
        /// <summary>
        /// Height of the status bar.
        /// </summary>
        private static int statusHeight = 5;

        /// <summary>
        /// Width of a standard tile.
        /// </summary>
        private static int tileWidth = 40;

        /// <summary>
        /// Height of a standard tile.
        /// </summary>
        private static int tileHeight = 18;

        /// <summary>
        /// Y coordinate where the tiles start, i.e., beneath the status bar.
        /// </summary>
        private static int tilesTop = GestureSpace.Top + statusHeight;

        /// <summary>
        /// Height of the header.
        /// </summary>
        private static int headerHeight = 12;

        /// <summary>
        /// Height of a message preview tile.
        /// </summary>
        private static int messageHeight = 12;

        /// <summary>
        /// Width of a full-width message;
        /// </summary>
        private static int messageWidth = 80;

        /// <summary>
        /// Width of a full-width button.
        /// </summary>
        private static int buttonWidth = 80;

        /// <summary>
        /// Width of a header button.
        /// </summary>
        private static int headerButtonWidth = 20;

        /// <summary>
        /// Height of a button in the detail view activities (ContactsItemActivity, InboxItemActivity, etc).
        /// </summary>
        private static int detailButtonHeight = 8;

        /// <summary>
        /// Height of the content detail in the detail view activities (ContactsItemActivity, InboxItemActivity, etc).
        /// </summary>
        private static int detailContentHeight = 27;
        #endregion

        #region Create ROIs
        /// <summary>
        /// Creates a region of interest at the given location in a grid interface.
        /// </summary>
        public static ROI CreateROI(String name, int number, int group, String toActivate, int row, int column)
        {
            int x = GestureSpace.Left + (column - 1) * tileWidth;
            int y = tilesTop +                  // Status bar
                    (row - 1) * tileHeight;     // Button depth

            return new ROI(new Rectangle(x, y, tileWidth, tileHeight), name, number, group, toActivate);
        }

        /// <summary>
        /// Creates a large region of interest at the given location in a grid interface.
        /// </summary>
        public static ROI CreateROI(String name, int number, int group, String toActivate, int row, int column, int columnSpan, int rowSpan)
        {
            int x = GestureSpace.Left + (column - 1) * tileWidth;
            int y = tilesTop +                  // Status bar
                    (row - 1) * tileHeight;     // Button depth

            return new ROI(new Rectangle(x, y, tileWidth * columnSpan, tileHeight * rowSpan), name, number, group, toActivate);
        }

        /// <summary>
        /// Creates a region of interest for a button in a list of buttons.
        /// </summary>
        public static ROI CreateButtonListROI(String name, int number, int group, String toActivate, int row)
        {
            int x = GestureSpace.Left;
            int y = tilesTop +                      // Status bar
                    headerHeight +                  // Header
                    (row - 1) * messageHeight;      // Button depth

            return new ROI(new Rectangle(x, y, messageWidth, messageHeight), name, number, group, toActivate);
        }

        /// <summary>
        /// Creates a region of interest for a button in the header.
        /// </summary>
        public static ROI CreateHeaderButtonROI(String name, int number, int group, String toActivate)
        {
            int x = GestureSpace.Left;
            int y = tilesTop;       // Status bar

            return new ROI(new Rectangle(x, y, headerButtonWidth, headerHeight), name, number, group, toActivate);
        }

        /// <summary>
        /// Creates a region of interest for a button in a detail level view activity,
        /// e.g. ContactsItemActivity and InboxItemActivity.
        /// </summary>
        public static ROI CreateDetailButtonROI(String name, int number, int group, String toActivate, int buttonNumber)
        {
            int x = GestureSpace.Left;
            int y = tilesTop +                                  // Status bar
                    headerHeight +                              // Header
                    detailContentHeight +                       // Content view
                    ((buttonNumber - 1) * detailButtonHeight);  // Button depth

            return new ROI(new Rectangle(x, y, buttonWidth, detailButtonHeight), name, number, group, toActivate);
        }
        #endregion
    }
}
