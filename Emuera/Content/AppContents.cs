﻿using MinorShift.Emuera.Sub;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MinorShift.Emuera.Content
{
	static class AppContents
	{
		static AppContents()
		{
			gList = new Dictionary<int, GraphicsImage>();
		}
		static Dictionary<string, AContentFile> resourceDic = new Dictionary<string, AContentFile>();
		static Dictionary<string, ASprite> imageDictionary = new Dictionary<string, ASprite>();
		static Dictionary<int, GraphicsImage> gList;

		//static public T GetContent<T>(string name)where T :AContentItem
		//{
		//	if (name == null)
		//		return null;
		//	name = name.ToUpper();
		//	if (!itemDic.ContainsKey(name))
		//		return null;
		//	return itemDic[name] as T;
		//}
		static public GraphicsImage GetGraphics(int i)
		{
			if (gList.ContainsKey(i))
				return gList[i];
			GraphicsImage g =  new GraphicsImage(i);
			gList[i] = g;
			return g;
		}

		static public ASprite GetSprite(string name)
		{
			if (name == null)
				return null;
			name = name.ToUpper();
			if (!imageDictionary.ContainsKey(name))
				return null;
			return imageDictionary[name];
		}

		static public void SpriteDispose(string name)
		{
			if (name == null)
				return;
			name = name.ToUpper();
			if (!imageDictionary.ContainsKey(name))
				return;
			imageDictionary[name].Dispose();
			imageDictionary.Remove(name);
		}

		static public void CreateSpriteG(string imgName, GraphicsImage parent,Rectangle rect)
		{
			if (string.IsNullOrEmpty(imgName))
				throw new ArgumentOutOfRangeException();
			imgName = imgName.ToUpper();
			SpriteG newCImg = new SpriteG(imgName, parent, rect);
			imageDictionary[imgName] = newCImg;
		}

		static public void LoadContents()
		{
			if (!Directory.Exists(Program.ContentDir))
				return;
			try
			{
				//string[] csvFiles = Directory.GetFiles(Program.ContentDir, "*.csv", SearchOption.TopDirectoryOnly);
				string[] csvFiles = Directory.GetFiles(Program.ContentDir, "*.csv", SearchOption.AllDirectories);
				foreach (var filename in csvFiles)
				{
					string directory = Path.GetDirectoryName(filename).ToUpper() + "\\";
					string[] lines = File.ReadAllLines(filename, Config.Encode);
					foreach (var line in lines)
					{
						if (line.Length == 0)
							continue;
						string str = line.Trim();
						if (str.Length == 0 || str.StartsWith(";"))
							continue;
						string[] tokens = str.Split(',');
						//AContentItem item = CreateFromCsv(tokens);

						ASprite item = CreateFromCsv(tokens, directory) as ASprite;
						if (item != null)
						{
							if (!imageDictionary.ContainsKey(item.Name))
								imageDictionary.Add(item.Name, item);
							else
								item.Dispose();
						}
					}
				}
			}
			catch
			{
				throw new CodeEE("リソースファイルのロード中にエラーが発生しました");
			}
		}

		static public void UnloadContents()
		{
			foreach (var img in resourceDic.Values)
				img.Dispose();
			resourceDic.Clear();
			imageDictionary.Clear();
		}

		static private AContentItem CreateFromCsv(string[] tokens, string dir)
		{
			if(tokens.Length < 2)
				return null;
			string name = tokens[0].Trim().ToUpper();//
			string parentName = dir + tokens[1].ToUpper();//画像ファイル名
			if (name.Length == 0 || parentName.Length == 0)
				return null;
			if (!resourceDic.ContainsKey(parentName))
			{
				string filepath = parentName;
				if (!File.Exists(filepath))
					return null;
				Bitmap bmp = new Bitmap(filepath);
				if (bmp == null)
					return null;
				ConstImage img = new ConstImage(parentName);
				img.CreateFrom(bmp, Config.TextDrawingMode == TextDrawingMode.WINAPI);
				if (!img.IsCreated)
					return null;
				resourceDic.Add(parentName, img);
			}
			AContentFile parent = resourceDic[parentName];
			if(parent is ConstImage)
			{
				ConstImage parentImage = parent as ConstImage;
				if (!parentImage.IsCreated)
						return null;
				Rectangle rect = new Rectangle(new Point(0, 0), parentImage.Bitmap.Size);
				Point pos = new Point();
				if(tokens.Length >= 6)
				{
					int[] rectValue = new int[4];
					bool sccs = true;
					for (int i = 0; i < 4; i++)
						sccs &= int.TryParse(tokens[i + 2], out rectValue[i]);
					if (sccs)
						rect = new Rectangle(rectValue[0], rectValue[1], rectValue[2], rectValue[3]);
					if(tokens.Length >= 8)
					{
						sccs = true;
						for (int i = 0; i < 2; i++)
							sccs &= int.TryParse(tokens[i + 6], out rectValue[i]);
						if (sccs)
							pos = new Point(rectValue[0], rectValue[1]);
					}
					//if(tokens.Length >= 7)
					//{
					//	string[] keywordTokens = tokens[6].Split('|');
					//	foreach(string keyword in keywordTokens)
					//	{
					//		switch(keyword.Trim().ToUpper())
					//		{
					//			case "NORESIZE":
					//				throw new NotImplCodeEE();
					//		}
					//	}
					//}
				}
				ASprite image = new SpriteF(name, parentImage, rect, pos);
				return image;
			}
			return null;
		}


	}
}
