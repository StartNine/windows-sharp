﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Timers;
using System.Xml.Linq;

namespace WindowsSharp.DiskItems
{
    public class AppInfo
    {
        /// <summary>
        /// Gets or sets the display name of the app.
        /// </summary>
        public String DisplayName { get; set; }

        /// <summary>
        /// Gets the path of the app.
        /// </summary>
        public String InternalPath
        {
            get => Environment.ExpandEnvironmentVariables(@"%programfiles%\WindowsApps\" + InternalName + @"\AppxManifest.xml");
        }

        /// <summary>
        /// Gets or sets the package name of the app.
        /// </summary>
        public String InternalName { get; set; }

        /// <summary>
        /// Gets or sets the color of the tile.
        /// </summary>
        public System.Drawing.Color Color { get; set; }

        /// <summary>
        /// Gets or sets the live text.
        /// </summary>
        public List<String> LiveText { get; set; }

        /// <summary>
        /// Gets or sets the live images.
        /// </summary>
        public List<String> LiveImagePaths { get; set; }

        XDocument _appxManifest; // TODO: use XDocument
                                 //%programfiles%/WindowsApps/Microsoft.BingNews_3.0.4.213_x64__8wekyb3d8bbwe/AppxManifest.xml

        NotificationInfo _currentNotification;

        public Timer _notificationTimer;

        /// <summary>
        /// Occurs when a tile update notification is received.
        /// </summary>
        public event EventHandler<NotificationInfoEventArgs> NotificationReceived;

        /// <summary>
        /// Creates an <see cref="AppInfo"/> object from the application package name.
        /// </summary>
        /// <param name="app">The application package name.</param>
        public AppInfo(String app)
        {
            InternalName = app;

            _appxManifest = XDocument.Parse(File.ReadAllText(InternalName));

            var dummyString = "If you see this text, remind me to look into getting Apps' display names.";
            DisplayName = dummyString;
            //XmlNodeList nodes = ;
            foreach (XElement e in _appxManifest.Descendants("DisplayName"))
            {
                if (e.NodeType.ToString().Contains("DisplayName"))
                {
                    DisplayName = e.Value;
                }
            }
            /*foreach (XmlNode node in _appxManifest.GetElementsByTagName("Package")[0].ChildNodes)
            {
                if (node.Name == "Properties")
                {
                    foreach (XmlNode property in node.ChildNodes)
                    {
                        if (property.Name == "DisplayName")
                        {
                            DisplayName = property.InnerText;
                        }
                    }
                }
            }*/

            /*if (DisplayName == dummyString)
            {
                foreach (XmlNode node in _appxManifest.GetElementsByTagName("Package")[0].ChildNodes)
                {
                    if ((node.Name == "Identity") && (node.Attributes["Name"] != null))
                    {
                        DisplayName = node.Attributes["Name"].Value;
                    }
                }
            }*/

            _notificationTimer = new Timer(5000);

            /*Icon = new ImageBrush(); //Get App icon here
            Color = Color.FromArgb(0xFF, 0xFF, 0, 0xFF); //Get App color here
            foreach (XmlNode node in _appxManifest.GetElementsByTagName("Package")[0].ChildNodes)
            {
                if (node.Name == "Applications")
                {
                    foreach (XmlNode secondNode in node.ChildNodes)
                    {
                        if (secondNode.Name == "Application")
                        {
                            foreach (XmlNode subNode in secondNode.ChildNodes)
                            {
                                if ((subNode.Name.ToLower().EndsWith("visualelements")) && (subNode.Attributes["BackgroundColor"] != null))
                                {
                                    var coloures = subNode.Attributes["BackgroundColor"].Value.Replace("#", "").ToUpper();
                                    Byte red = 0;
                                    Byte green = 0;
                                    Byte blue = 0;
                                    if (coloures.Length == 6)
                                    {
                                        red = byte.Parse(coloures.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                                        green = byte.Parse(coloures.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                                        blue = byte.Parse(coloures.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                                    }
                                    else if (coloures.Length == 8)
                                    {
                                        red = byte.Parse(coloures.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                                        green = byte.Parse(coloures.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                                        blue = byte.Parse(coloures.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                                    }
                                    Color = Color.FromArgb(0xFF, red, green, blue);
                                }
                            }
                        }
                    }
                }
            }*/
            //"Microsoft.BingSports_3.0.4.212_x64__8wekyb3d8bbwe"

            _currentNotification = CurrentLiveTileNotification;
            _notificationTimer.Elapsed += (sneder, args) =>
            {
                /*Dispatcher.Invoke(new Action(() =>
                {*/
                var NewNotification = CurrentLiveTileNotification;
                if (_currentNotification != NewNotification)
                {
                    NotificationReceived?.Invoke(this, new NotificationInfoEventArgs()
                    {
                        OldNotification = _currentNotification,
                        NewNotification = NewNotification
                    });
                    _currentNotification = NewNotification;
                    LiveText.Clear();
                    foreach (var s in NewNotification.Text)
                    {
                        LiveText.Add(s);
                    }
                    LiveImagePaths.Clear();
                    foreach (var b in NewNotification.ImagePaths)
                    {
                        LiveImagePaths.Add(b);
                    }
                }
                //}));
            };
            _notificationTimer.Start();
        }

        String TileNotificationAddress
        {
            get
            {
                var address = "";
                //XmlDocument appxManifest = new XmlDocument();
                //%programfiles%/WindowsApps/Microsoft.BingNews_3.0.4.213_x64__8wekyb3d8bbwe/AppxManifest.xml
                //appxManifest.Load(InternalPath);
                /*foreach (XmlNode node in _appxManifest.GetElementsByTagName("Package")[0].ChildNodes)
                {
                    if (node.Name.ToLower() == "applications")
                    {
                        foreach (XmlNode secondNode in node.ChildNodes)
                        {
                            if (secondNode.Name.ToLower() == "application")
                            {
                                foreach (XmlNode subNode in secondNode.ChildNodes)
                                {
                                    if (subNode.Name.ToLower().EndsWith("visualelements"))
                                    {
                                        foreach (XmlNode defaultTileNode in subNode.ChildNodes)
                                        {
                                            if (defaultTileNode.Name.ToLower().EndsWith("defaulttile"))
                                            {
                                                foreach (XmlNode tileUpdNode in defaultTileNode.ChildNodes)
                                                {
                                                    if ((tileUpdNode.Name.ToLower().EndsWith("tileupdate")) && (tileUpdNode.Attributes["UriTemplate"] != null))
                                                    {
                                                        address = tileUpdNode.Attributes["UriTemplate"].Value;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }*/
                foreach (XElement x in _appxManifest.Descendants("tileupdate"))
                {
                    if (x.Attribute("UriTemplate") != null)
                    {
                        address = x.Attribute("UriTemplate").Value;
                    }
                }
                return address;
            }
        }

        NotificationInfo CurrentLiveTileNotification
        {
            get
            {
                NotificationInfo notifyInfo = new NotificationInfo();
                System.Xml.XmlDocument tileDocument = new System.Xml.XmlDocument();

                tileDocument.LoadXml(NotificationInfo.GetLiveTileFromWebAddress(TileNotificationAddress));

                foreach (System.Xml.XmlNode visual in tileDocument.SelectNodes("/tile/visual"))
                {
                    foreach (System.Xml.XmlNode binding in visual.ChildNodes)
                    {
                        foreach (System.Xml.XmlNode node in binding.ChildNodes)
                        {
                            if (node.Name.ToLower() == "image")
                            {
                                if (Uri.IsWellFormedUriString(node.Attributes["src"].Value, UriKind.RelativeOrAbsolute))
                                {
                                    notifyInfo.ImagePaths.Add(node.Attributes["src"].Value);
                                }
                                else if (Uri.IsWellFormedUriString(node.Attributes["alt"].Value, UriKind.RelativeOrAbsolute))
                                {
                                    notifyInfo.ImagePaths.Add(node.Attributes["alt"].Value);
                                }
                            }
                            else if (node.Name.ToLower() == "text")
                            {
                                notifyInfo.Text.Add(Encoding.Default.GetString(Encoding.Convert(Encoding.UTF8, Encoding.Default, Encoding.Default.GetBytes(node.InnerText))));
                            }
                        }
                        try
                        {
                            notifyInfo.Type = (NotificationInfo.TileNotificationType)Enum.Parse(typeof(NotificationInfo.TileNotificationType), binding.Attributes["template"].Value, true);
                        }
                        catch
                        {
                            try
                            {
                                notifyInfo.Type = (NotificationInfo.TileNotificationType)Enum.Parse(typeof(NotificationInfo.TileNotificationType), binding.Attributes["fallback"].Value, true);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }
                    }
                }

                return notifyInfo;
            }
        }
    }

    public class NotificationInfoEventArgs : EventArgs //RoutedEventArgs?
    {
        public NotificationInfo OldNotification;
        public NotificationInfo NewNotification;
    }

    /// <summary>
    /// Represents a live tile update notification.
    /// </summary>
    public class NotificationInfo
    {
        public enum TileNotificationType
        {
            TileSquare150x150Block,
            TileSquare150x150IconWithBadge,
            TileSquare150x150Image,
            TileSquare150x150PeekImageAndText01,
            TileSquare150x150PeekImageAndText02,
            TileSquare150x150PeekImageAndText03,
            TileSquare150x150PeekImageAndText04,
            TileSquare150x150Text01,
            TileSquare150x150Text02,
            TileSquare150x150Text03,
            TileSquare150x150Text04,
            TileSquare310x310BlockAndText01,
            TileSquare310x310BlockAndText02,
            TileSquare310x310Image,
            TileSquare310x310ImageAndText01,
            TileSquare310x310ImageAndText02,
            TileSquare310x310ImageAndTextOverlay01,
            TileSquare310x310ImageAndTextOverlay02,
            TileSquare310x310ImageAndTextOverlay03,
            TileSquare310x310ImageCollection,
            TileSquare310x310ImageCollectionAndText01,
            TileSquare310x310ImageCollectionAndText02,
            TileSquare310x310SmallImageAndText01,
            TileSquare310x310SmallImagesAndTextList01,
            TileSquare310x310SmallImagesAndTextList02,
            TileSquare310x310SmallImagesAndTextList03,
            TileSquare310x310SmallImagesAndTextList04,
            TileSquare310x310SmallImagesAndTextList05,
            TileSquare310x310Text01,
            TileSquare310x310Text02,
            TileSquare310x310Text03,
            TileSquare310x310Text04,
            TileSquare310x310Text05,
            TileSquare310x310Text06,
            TileSquare310x310Text07,
            TileSquare310x310Text08,
            TileSquare310x310Text09,
            TileSquare310x310TextList01,
            TileSquare310x310TextList02,
            TileSquare310x310TextList03,
            TileSquareBlock,
            TileSquareImage,
            TileSquarePeekImageAndText01,
            TileSquarePeekImageAndText02,
            TileSquarePeekImageAndText03,
            TileSquarePeekImageAndText04,
            TileSquareText01,
            TileSquareText02,
            TileSquareText03,
            TileSquareText04,
            TileTall150x310Image,
            TileWide310x150BlockAndText01,
            TileWide310x150BlockAndText02,
            TileWide310x150IconWithBadgeAndText,
            TileWide310x150Image,
            TileWide310x150ImageAndText01,
            TileWide310x150ImageAndText02,
            TileWide310x150ImageCollection,
            TileWide310x150PeekImage01,
            TileWide310x150PeekImage02,
            TileWide310x150PeekImage03,
            TileWide310x150PeekImage04,
            TileWide310x150PeekImage05,
            TileWide310x150PeekImage06,
            TileWide310x150PeekImageAndText01,
            TileWide310x150PeekImageAndText02,
            TileWide310x150PeekImageCollection01,
            TileWide310x150PeekImageCollection02,
            TileWide310x150PeekImageCollection03,
            TileWide310x150PeekImageCollection04,
            TileWide310x150PeekImageCollection05,
            TileWide310x150PeekImageCollection06,
            TileWide310x150SmallImageAndText01,
            TileWide310x150SmallImageAndText02,
            TileWide310x150SmallImageAndText03,
            TileWide310x150SmallImageAndText04,
            TileWide310x150SmallImageAndText05,
            TileWide310x150Text01,
            TileWide310x150Text02,
            TileWide310x150Text03,
            TileWide310x150Text04,
            TileWide310x150Text05,
            TileWide310x150Text06,
            TileWide310x150Text07,
            TileWide310x150Text08,
            TileWide310x150Text09,
            TileWide310x150Text10,
            TileWide310x150Text11,
            TileWideBlockAndText01,
            TileWideBlockAndText02,
            TileWideImage,
            TileWideImageAndText01,
            TileWideImageAndText02,
            TileWideImageCollection,
            TileWidePeekImage01,
            TileWidePeekImage02,
            TileWidePeekImage03,
            TileWidePeekImage04,
            TileWidePeekImage05,
            TileWidePeekImage06,
            TileWidePeekImageAndText01,
            TileWidePeekImageAndText02,
            TileWidePeekImageCollection01,
            TileWidePeekImageCollection02,
            TileWidePeekImageCollection03,
            TileWidePeekImageCollection04,
            TileWidePeekImageCollection05,
            TileWidePeekImageCollection06,
            TileWideSmallImageAndText01,
            TileWideSmallImageAndText02,
            TileWideSmallImageAndText03,
            TileWideSmallImageAndText04,
            TileWideSmallImageAndText05,
            TileWideText01,
            TileWideText02,
            TileWideText03,
            TileWideText04,
            TileWideText05,
            TileWideText06,
            TileWideText07,
            TileWideText08,
            TileWideText09,
            TileWideText10,
            TileWideText11
        }

        /// <summary>
        /// Gets a live tile from the address.
        /// </summary>
        /// <param name="address">The address to get the tile from.</param>
        /// <returns>The live tile address.</returns>
        public static String GetLiveTileFromWebAddress(String address)
        {
            String tempUri;
            using (var wc = new System.Net.WebClient())
            {
                var tempUriThingy = address.Replace("{language}", "en-US"); //TODO: Don't hardcode US English
                tempUri = wc.DownloadString(tempUriThingy).Replace("{language}", "en-US"); //TODO: Don't hardcode US English
            }
            return tempUri;
        }

        /// <summary>
        /// Gets or sets the type of the tile notification.
        /// </summary>
        /// <value>
        /// A <see cref="TileNotificationType"/> representing the notification type.
        /// </value>
        public TileNotificationType Type { get; set; }

        /// <summary>
        /// Gets a list of text strings in the tile update.
        /// </summary>
        /// <value>
        /// A list of the text strings found in the notification.
        /// </value>
        public List<String> Text { get; } = new List<String>();

        /// <summary>
        /// Gets a list of images in the tile update.
        /// </summary>
        /// <value>
        /// A list of <see cref="ImageBrush"/>es that can paint the images found in the notification.
        /// </value>
        public List<String> ImagePaths { get; } = new List<String>();
    }
}