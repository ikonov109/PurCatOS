using System;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.ExtendedASCII;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using Console = System.Console;

namespace PurcatOS
{
    public class Kernel : Cosmos.System.Kernel
    {
        private string activePath = @"0:\";

        protected override void BeforeRun()
        {
            try { Encoding.RegisterProvider(CosmosEncodingProvider.Instance); } catch { }

            try
            {
                var fs = new CosmosVFS();
                VFSManager.RegisterVFS(fs);
            }
            catch
            {
                Console.WriteLine("VFS Init Failed.");
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"  _____                     _     ____  _____ ");
            Console.WriteLine(@" |  __ \                   | |   / __ \/ ____|");
            Console.WriteLine(@" | |__) |   _ _ __ ___ __ _| |_ | |  | \___ \ ");
            Console.WriteLine(@" |  ___/ | | | '__/ __/ _` | __|| |  | |___) |");
            Console.WriteLine(@" | |   | |_| | | | (_| (_| | |_ | |__| |__/ / ");
            Console.WriteLine(@" |_|    \__,_|_|  \___\__,_|\__| \____/|_____/ ");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Welcome to Purcat OS v4.1 (Disk Tools)!");
            Console.WriteLine(" Type 'help' for commands.");
            Console.WriteLine("=====================================================");
            Console.ResetColor();
        }

        protected override void Run()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Purcat> ");
            Console.ResetColor();

            string input = Console.ReadLine();
            if (input == null || input.Length == 0) return;

            string[] parts = input.Trim().Split(' ');
            string command = parts[0].ToLower();

            // --- ПОЛНЫЙ СПИСОК КОМАНД ---
            if (command == "help")
            {
                Console.WriteLine(" partlist        - Show disks and their format status");
                Console.WriteLine(" formatdisk <N>  - Format disk by index (e.g. formatdisk 1)");
                Console.WriteLine(" time / date     - Show time/date");
                Console.WriteLine(" calc <a> <op> <b>- Calculator");
                Console.WriteLine(" dir             - List files");
                Console.WriteLine(" cdir <name>     - Create a folder");
                Console.WriteLine(" edit <file>     - Create/Edit file");
                Console.WriteLine(" setdir <path>   - Change path (e.g. setdir 1:\\)");
                Console.WriteLine(" fm              - File Manager (F2 to switch disk)");
                Console.WriteLine(" gui             - Graphic Mode");
                Console.WriteLine(" bsod            - Simulate BSOD");
                Console.WriteLine(" reboot/shutdown - Power options");
            }
            else if (command == "partlist")
            {
                Console.WriteLine("--- DISKS ---");
                var disks = VFSManager.GetDisks();
                if (disks.Count == 0) Console.WriteLine("No disks found.");
                for (int i = 0; i < disks.Count; i++)
                {
                    Console.WriteLine("Disk " + i + ": Size = " + (disks[i].Size / 1024 / 1024) + " MB, Partitions = " + disks[i].Partitions.Count);
                }

                Console.WriteLine("--- DRIVE STATUS ---");
                for (int i = 0; i < 4; i++)
                {
                    string drive = i + ":\\";
                    try
                    {
                        VFSManager.GetDirectoryListing(drive);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(drive + " : READY (Formatted)");
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(drive + " : NOT READY (CD-ROM or Unformatted)");
                    }
                    Console.ResetColor();
                }
            }
            else if (command == "formatdisk")
            {
                int diskIndex = 0;
                if (parts.Length > 1) int.TryParse(parts[1], out diskIndex);

                try
                {
                    var disks = VFSManager.GetDisks();
                    if (diskIndex >= disks.Count)
                    {
                        Console.WriteLine("Error: Disk " + diskIndex + " not found.");
                        return;
                    }

                    var targetDisk = disks[diskIndex];
                    Console.WriteLine("Formatting Disk " + diskIndex + " (" + (targetDisk.Size / 1024 / 1024) + " MB)...");

                    if (targetDisk.Partitions.Count == 0)
                        targetDisk.CreatePartition(targetDisk.Size);

                    targetDisk.FormatPartition(0, "FAT32", true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Format successful! Disk " + diskIndex + " is now FAT32.");
                    Console.ResetColor();
                }
                catch
                {
                    Console.WriteLine("Format Error. Make sure it's a Hard Disk, not CD-ROM.");
                }
            }
            else if (command == "cls" || command == "clear")
            {
                Console.Clear();
            }
            else if (command == "echo")
            {
                for (int i = 1; i < parts.Length; i++) Console.Write(parts[i] + " ");
                Console.WriteLine();
            }
            else if (command == "time")
            {
                Console.WriteLine("Time: " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
            }
            else if (command == "date")
            {
                Console.WriteLine("Date: " + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year);
            }
            else if (command == "calc")
            {
                if (parts.Length == 4)
                {
                    try
                    {
                        double a = double.Parse(parts[1]);
                        double b = double.Parse(parts[3]);
                        string op = parts[2];
                        double result = 0;

                        if (op == "+") result = a + b;
                        else if (op == "-") result = a - b;
                        else if (op == "*") result = a * b;
                        else if (op == "/")
                        {
                            if (b == 0) { Console.WriteLine("Error: Div by zero!"); return; }
                            result = a / b;
                        }
                        else { Console.WriteLine("Use +, -, * or /"); return; }

                        Console.WriteLine("Result: " + result);
                    }
                    catch { Console.WriteLine("Format error."); }
                }
                else Console.WriteLine("Usage: calc <num1> <op> <num2>");
            }
            else if (command == "setdir")
            {
                if (parts.Length > 1)
                {
                    activePath = parts[1];
                    if (!activePath.EndsWith("\\")) activePath = activePath + "\\";
                    Console.WriteLine("Path changed to: " + activePath);
                }
            }
            else if (command == "dir")
            {
                try
                {
                    var entries = VFSManager.GetDirectoryListing(activePath);
                    Console.WriteLine("Directory of " + activePath);
                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (entries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        else
                            Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("  " + entries[i].mName);
                    }
                    Console.ResetColor();
                }
                catch { Console.WriteLine("[Disk Error] Cannot read directory."); }
            }
            else if (command == "cdir")
            {
                if (parts.Length > 1)
                {
                    try { VFSManager.CreateDirectory(activePath + parts[1]); Console.WriteLine("Directory created!"); }
                    catch { Console.WriteLine("[Error] Cannot create directory."); }
                }
                else Console.WriteLine("Usage: cdir <foldername>");
            }
            else if (command == "edit")
            {
                if (parts.Length < 2) { Console.WriteLine("Usage: edit <filename>"); return; }
                TextEditor(activePath + parts[1]);
            }
            else if (command == "fm")
            {
                FileManager();
            }
            else if (command == "gui")
            {
                StartGUI();
            }
            else if (command == "bsod")
            {
                ShowBSOD();
            }
            else if (command == "reboot")
            {
                Power.Reboot();
            }
            else if (command == "shutdown" || command == "halt")
            {
                Power.Shutdown();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Command '" + command + "' not recognized.");
                Console.ResetColor();
            }
        }

        private void TextEditor(string filePath)
        {
            Console.Clear();
            Console.WriteLine("--- PURCAT EDITOR: " + filePath + " ---");
            Console.WriteLine("(Ctrl+S to save, Esc to exit)\n");

            string content = "";
            try
            {
                var file = VFSManager.GetFile(filePath);
                if (file != null)
                {
                    var stream = file.GetFileStream();
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    content = Encoding.ASCII.GetString(buffer);
                    stream.Close();
                }
            }
            catch { Console.WriteLine("[File not found, starting new]"); }

            Console.Write(content);

            while (true)
            {
                KeyEvent keyEvent;
                while (!KeyboardManager.TryReadKey(out keyEvent)) { }

                if (keyEvent.Key == ConsoleKeyEx.Escape) break;

                if (keyEvent.Key == ConsoleKeyEx.S && KeyboardManager.ControlPressed)
                {
                    try
                    {
                        var file = VFSManager.CreateFile(filePath);
                        if (file != null)
                        {
                            var stream = file.GetFileStream();
                            byte[] bytes = Encoding.ASCII.GetBytes(content);
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Close();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n\n[Saved!]");
                        }
                    }
                    catch { Console.WriteLine("\n[Save Error] Disk is read-only."); }
                    Console.ResetColor();
                    Console.ReadKey();
                    break;
                }

                if (keyEvent.Key == ConsoleKeyEx.Backspace)
                {
                    if (content.Length > 0)
                    {
                        content = content.Substring(0, content.Length - 1);
                        Console.Clear();
                        Console.WriteLine("--- PURCAT EDITOR: " + filePath + " ---");
                        Console.WriteLine("(Ctrl+S to save, Esc to exit)\n");
                        Console.Write(content);
                    }
                }
                else if (keyEvent.Key == ConsoleKeyEx.Enter)
                {
                    content += "\n";
                    Console.WriteLine();
                }
                else if (keyEvent.KeyChar != '\0')
                {
                    content += keyEvent.KeyChar;
                    Console.Write(keyEvent.KeyChar);
                }
            }
            Console.ResetColor();
        }

        private void FileManager()
        {
            string currentPath = activePath;
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" PURCAT FM | Path: " + currentPath + " | F2: Change Disk ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                List<DirectoryEntry> files = new List<DirectoryEntry>();
                bool hasError = false;

                try { files = VFSManager.GetDirectoryListing(currentPath); }
                catch { hasError = true; }

                if (hasError)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nCannot read disk " + currentPath);
                    Console.WriteLine("Press F2 to change disk (0:\\, 1:\\, 2:\\) or Esc to exit.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    if (files.Count == 0) Console.WriteLine("\n [Empty Directory] ");

                    for (int i = 0; i < files.Count; i++)
                    {
                        if (i == selectedIndex)
                        {
                            Console.BackgroundColor = ConsoleColor.Cyan;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            if (files[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            else
                                Console.ForegroundColor = ConsoleColor.White;
                        }

                        if (files[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                            Console.WriteLine(" [DIR] " + files[i].mName);
                        else
                            Console.WriteLine("       " + files[i].mName);
                    }
                }

                KeyEvent keyEvent;
                while (!KeyboardManager.TryReadKey(out keyEvent)) { }

                if (keyEvent.Key == ConsoleKeyEx.Escape) return;

                if (keyEvent.Key == ConsoleKeyEx.F2)
                {
                    if (currentPath.StartsWith("0")) currentPath = "1:\\";
                    else if (currentPath.StartsWith("1")) currentPath = "2:\\";
                    else currentPath = "0:\\";
                    activePath = currentPath;
                    selectedIndex = 0;
                    continue;
                }

                if (!hasError)
                {
                    if (keyEvent.Key == ConsoleKeyEx.UpArrow && selectedIndex > 0) selectedIndex--;
                    if (keyEvent.Key == ConsoleKeyEx.DownArrow && selectedIndex < files.Count - 1) selectedIndex++;

                    if (keyEvent.Key == ConsoleKeyEx.Enter && files.Count > 0)
                    {
                        var sel = files[selectedIndex];
                        if (sel.mEntryType == DirectoryEntryTypeEnum.Directory)
                        {
                            if (sel.mName != "." && sel.mName != "..")
                            {
                                currentPath = currentPath + sel.mName + "\\";
                                selectedIndex = 0;
                            }
                        }
                        else TextEditor(currentPath + sel.mName);
                    }
                }
            }
        }

        private void ShowBSOD()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            for (int i = 0; i < Console.WindowHeight; i++) Console.WriteLine(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("\n\n\n   :(   PC Crashed");
            Console.WriteLine("\n   ERR_OS_SETUP_CRASHED");
            Console.WriteLine("\n\n   Press any key to reboot...");
            Console.ReadKey();
            Console.ResetColor();
            Power.Reboot();
        }

        private void StartGUI()
        {
            try
            {
                Canvas canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(800, 600, ColorDepth.ColorDepth32));
                var myFont = PCScreenFont.Default;
                var whitePen = new Pen(Color.White);
                var cyanPen = new Pen(Color.Cyan);
                var redPen = new Pen(Color.Red);
                var grayPen = new Pen(Color.Gray);

                string[] menuItems = { "Ruskiy", "English", "Vihod (Exit)" };
                int selected = 0;

                while (true)
                {
                    canvas.Clear(Color.Black);
                    canvas.DrawString("Purcat OS GUI Mode", myFont, cyanPen, 250, 50);
                    canvas.DrawString("Select language:", myFont, whitePen, 300, 100);

                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        int y = 200 + (i * 70);
                        var btnPen = (i == selected) ? redPen : grayPen;
                        canvas.DrawFilledRectangle(btnPen, 250, y, 300, 50);
                        canvas.DrawString(menuItems[i], myFont, whitePen, 270, y + 15);
                    }
                    canvas.Display();

                    if (KeyboardManager.TryReadKey(out KeyEvent key))
                    {
                        if (key.Key == ConsoleKeyEx.UpArrow) selected--;
                        if (key.Key == ConsoleKeyEx.DownArrow) selected++;
                        if (selected < 0) selected = menuItems.Length - 1;
                        if (selected >= menuItems.Length) selected = 0;
                        if (key.Key == ConsoleKeyEx.Enter && selected == 2) Power.Reboot();
                    }
                }
            }
            catch { Console.WriteLine("GUI Failed."); Console.ReadKey(); }
        }
    }
}