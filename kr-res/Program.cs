using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr_res
{
	class RcDesc
	{
		public string spriteSize;
		public bool spriteTrimmed;
		public string[] aliases;
		public string spriteColorRect;
		public string spriteOffset;
		public string textureRect;
		public bool textureRotated;
		public string spriteSourceSize;

		//"spriteSize":"{18, 24}",
		//"spriteTrimmed":true,
		//"aliases":[],
		//"spriteColorRect": "{{14, 4}, {18, 24}}",
		//"spriteOffset":"{0, -0}",
		//"textureRect":"{{364, 276}, {18, 24}}",
		//"textureRotated":false,
		//"spriteSourceSize":"{46, 32}"
	}

	class Program
	{
		static string[] help_strings = {"help", "-help", "--help", "-h", "--h", "/h"};
		static void Main(string[] args)
		{
			//"c:\workdir\ext\enemies_goblins.plist"
			//"c:\workdir\ext\enemies_goblins.png"
			//string default_out_dir = "c:\\workdir\\ext\\processed_resources\\";
			//string default_png_file = "c:\\workdir\\ext\\heroes_thor.png";
			//string default_json_file = "c:\\workdir\\ext\\heroes_thor.plist";

			if (args.Length == 0 || HelpRequested(args[0]))
			{
				WriteHelp();
				PressAnyKey();
				return;
			}

			string work_dir = AppDomain.CurrentDomain.BaseDirectory;
			Console.WriteLine("current dir: " + work_dir);

			string rc_name = args[0];
			string png_file = work_dir + rc_name + ".png";
			string json_file = work_dir + rc_name + ".plist";
			if (!File.Exists(png_file))
			{
				Console.WriteLine("File not found: " + png_file);
				PressAnyKey();
				return;
			}
			if (!File.Exists(json_file))
			{
				Console.WriteLine("File not found: " + json_file);
				PressAnyKey();
				return;
			}

			string out_dir = work_dir + "processed_resources\\" + rc_name + "\\";
			Directory.CreateDirectory(out_dir);

			//string default_png_file = "c:\\workdir\\ext\\towers.png";
			//string default_json_file = "c:\\workdir\\ext\\towers.plist";
			//string default_png_file = "c:\\workdir\\ext\\sprite_level8_2.png";
			//string default_json_file = "c:\\workdir\\ext\\sprite_level8_2.plist";
			//string default_png_file = "c:\\workdir\\ext\\enemies_goblins.png";
			//string default_json_file = "c:\\workdir\\ext\\enemies_goblins.plist";
			Bitmap src = Image.FromFile(png_file) as Bitmap;
			if (src != null)
			{
				using (StreamReader stream = new StreamReader(json_file))
				{
					using (JsonTextReader reader = new JsonTextReader(stream))
					{
						JObject rootObj = (JObject)JToken.ReadFrom(reader);
						JToken frames = rootObj["frames"];
						if (frames != null)
						{
							foreach (JProperty property in frames)
							{
								Console.WriteLine(property.Name);
								RcDesc rc_desc = property.First.ToObject<RcDesc>();

								//RectangleConverter r = new RectangleConverter();
								//Rectangle tex_rect = (Rectangle)r.ConvertFromString(rc_desc.textureRect);

								Rectangle tex_rect = new Rectangle();

								String tmp = rc_desc.textureRect.Substring(1, rc_desc.textureRect.Length - 2);
								string[] elems = tmp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
								for (int i = 0; i < elems.Length; i++)
									elems[i] = elems[i].Trim();
								elems[0] = elems[0].Substring(1, elems[0].Length - 1);
								tex_rect.X = ParseInt(elems[0]);
								//tex_rect.X = Int32.Parse(elems[0]);
								elems[1] = elems[1].Substring(0, elems[1].Length - 1);
								tex_rect.Y = ParseInt(elems[1]);
								//tex_rect.Y = Int32.Parse(elems[1]);
								elems[2] = elems[2].Substring(1, elems[2].Length - 1);
								tex_rect.Width = ParseInt(elems[2]);
								//tex_rect.Width = Int32.Parse(elems[2]);
								elems[3] = elems[3].Substring(0, elems[3].Length - 1);
								tex_rect.Height = ParseInt(elems[3]);
								//tex_rect.Height = Int32.Parse(elems[3]);

								//TODO: add support for rc_desc.textureRotated here
								if (tex_rect != null)
								{
									Bitmap croppedImage = src.Clone(tex_rect, src.PixelFormat);
									croppedImage.Save(out_dir + property.Name, ImageFormat.Png);
								}
							}
						}
						else
							Console.WriteLine("Json from file " + json_file + " is missing root object frames!");
					}
				}
			}
			else
				Console.WriteLine("Failed load file " + png_file);

			PressAnyKey();
		}

		static Int32 ParseInt(string val)
		{
			Int32 result;
			if (Int32.TryParse(val, out result))
				return result;

			return (Int32)float.Parse(val, CultureInfo.InvariantCulture.NumberFormat);
		}

		static bool HelpRequested(string rc_str)
		{
			for (int i = 0; i < help_strings.Length; i++)
				if (rc_str == help_strings[i])
					return true;
			return false;
		}

		static void WriteHelp()
		{
			Console.WriteLine("KR resource extractor");
			Console.WriteLine("Usage: kr-res <rc_name>");
			Console.WriteLine("It searches for <rc_name>.png and <rc_name>.plist in current dir");
			Console.WriteLine("Creates out dir named <current_dir>//processed_resources//<rc_name>");
			Console.WriteLine("And output every frame from png file, as described in plist file to out dir");
		}

		static void PressAnyKey()
		{
			Console.WriteLine("Press any key to continue ...");
			Console.ReadKey();
		}
	}
}
