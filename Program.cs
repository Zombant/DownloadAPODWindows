using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace DownloadAPOD {
    class Program {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void Main(string[] args) {

            string htmlFileLocation = @"apod.txt";
            string configFileLocation = @"config.txt";
            string imageFileName = "apod.jpg";


            string imageFileLocation = "";
            if (!File.Exists(configFileLocation)) {
                Console.WriteLine("Enter the save location for the image or leave blank to store in program directory.");
                File.WriteAllText(configFileLocation, Console.ReadLine());
                if (!File.ReadAllText(configFileLocation).EndsWith("\\") && !File.ReadAllText(configFileLocation).Equals("")) {
                    File.WriteAllText(configFileLocation, File.ReadAllText(configFileLocation) + "\\");
                }
            }
            
            imageFileLocation = File.ReadAllText(configFileLocation);
            if (imageFileLocation.Equals("")) {
                imageFileLocation = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
                File.WriteAllText(configFileLocation,imageFileLocation);
            }

            //Set up dateString
            DateTime date = DateTime.Now;
            string dateString = date.Year + "";
            dateString = dateString.Substring(2, dateString.Length - 2);
            switch (date.Month) {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    dateString = dateString + "0" + date.Month;
                    break;
                case 10:
                case 11:
                case 12:
                    dateString = dateString + date.Month;
                    break;
            }
            if(date.Day >= 1 && date.Day <= 9) {
                dateString = dateString + "0" + date.Day; 
            }
            else {
                dateString = dateString + date.Day;
            }

            //Use dateString to make apodImageToDownload
            string apodImageToDownloadHTML = "https://apod.nasa.gov/apod/ap" + dateString +".html";


            //Download HTML as TXT
            using(WebClient client = new WebClient()) {
                client.DownloadFile(apodImageToDownloadHTML, htmlFileLocation);
            }
            Console.WriteLine("Downloading HTML from:  " + apodImageToDownloadHTML);


            //Find part of TXT that has the image url
            string wantedLine = "";
            foreach(var line in File.ReadAllLines(htmlFileLocation)) {
                if(line.Contains("<a href=\"image")) {
                    wantedLine = line;
                    break;
                }
            }


            //Stop execution if there is no image
            if (wantedLine == "") {
                Console.WriteLine("\n\nNo image found on " + apodImageToDownloadHTML);
                Console.WriteLine("Try again tomorrow!");
                Console.ReadLine();
                return;
            }

            //Remove unwanted parts from URL ending
            wantedLine = wantedLine.Trim();
            int beginningIndex = wantedLine.IndexOf('i');
            string imageURLEnd = wantedLine.Substring(beginningIndex, wantedLine.Length - beginningIndex - 2);
            Console.WriteLine("Image Downloading:      " + imageURLEnd);

            //Download Image
            string imageURL = "https://apod.nasa.gov/apod/" + imageURLEnd;
            Console.WriteLine("Image URL:              " + imageURL);
            using (WebClient client = new WebClient()) {
                client.DownloadFile(imageURL, imageFileLocation + imageFileName);
            }

            //Delete HTML file
            File.Delete(htmlFileLocation);
            //Console.WriteLine("\nDeleting:               " + htmlFileLocation);
            Console.WriteLine("Image saved to " + imageFileLocation);
            //Console.ReadLine();

            //Set wallpaper
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", 10.ToString());
            key.SetValue(@"TileWallpaper", 0.ToString());
            SystemParametersInfo(20, 0, imageFileLocation + imageFileName, 0x01 | 0x02);

        }

        
    }
}

/*
if (style == Style.Fill) {
    key.SetValue(@"WallpaperStyle", 10.ToString());
    key.SetValue(@"TileWallpaper", 0.ToString());
}
if (style == Style.Fit) {
    key.SetValue(@"WallpaperStyle", 6.ToString());
    key.SetValue(@"TileWallpaper", 0.ToString());
}
if (style == Style.Span) // Windows 8 or newer only!
{
    key.SetValue(@"WallpaperStyle", 22.ToString());
    key.SetValue(@"TileWallpaper", 0.ToString());
}
if (style == Style.Stretch) {
    key.SetValue(@"WallpaperStyle", 2.ToString());
    key.SetValue(@"TileWallpaper", 0.ToString());
}
if (style == Style.Tile) {
    key.SetValue(@"WallpaperStyle", 0.ToString());
    key.SetValue(@"TileWallpaper", 1.ToString());
}
if (style == Style.Center) {
    key.SetValue(@"WallpaperStyle", 0.ToString());
    key.SetValue(@"TileWallpaper", 0.ToString());
}
*/