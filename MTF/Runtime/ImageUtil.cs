
using System;
using System.Collections.Generic;
using System.Drawing;
using MTF.PropertyValues;
using TexPacker;
using UnityEngine;

namespace MTF
{
	public class ImageChannelSetup
	{
		public class ImageChannel
		{
			public IPropertyValue Source;
			public bool Invert;
			public ImageChannel(IPropertyValue Source, bool Invert = false) { this.Source = Source; this.Invert = Invert; }
			public static ImageChannel Empty() { return new ImageChannel(null); }
		}
		public string Name;
		public ImageChannel Channel0;
		public ImageChannel Channel1;
		public ImageChannel Channel2;
		public ImageChannel Channel3;

		public ImageChannelSetup() {}

		public ImageChannelSetup(ImageChannel Channel0, ImageChannel Channel1, ImageChannel Channel2, ImageChannel Channel3)
		{
			this.Channel0 = Channel0; this.Channel1 = Channel1; this.Channel2 = Channel2; this.Channel3 = Channel3;
		}

		public ImageChannel this[int key]
		{
			get {
				switch(key)
				{
					case 0: return Channel0;
					case 1: return Channel1;
					case 2: return Channel2;
					case 3: return Channel3;
					default: throw new Exception("Invalid Channel Access!");
				}
			}
		}
	}

	public class ImageUtil
	{
		private static TextureChannel ChannelIdxToEnum(int Idx)
		{
			switch(Idx)
			{
				case 0: return TextureChannel.ChannelRed;
				case 1: return TextureChannel.ChannelGreen;
				case 2: return TextureChannel.ChannelBlue;
				case 3: return TextureChannel.ChannelAlpha;
				default: throw new Exception("Invalid Channel Access!");
			}
		}
		
		public static Texture2D AssembleTextureChannels(ImageChannelSetup Channels)
		{
			var packer = new TexturePacker();
			packer.Initialize();
			for(int i = 0; i < 4; i++)
			{
				var channelSource = Channels[i];
				if(channelSource.Source != null)
				{
					var input = new TextureInput();
					switch(channelSource.Source.Type)
					{
						case TexturePropertyValue._TYPE:
							input.texture = ((TexturePropertyValue)channelSource.Source).Texture;
							input.SetChannelInput(ChannelIdxToEnum(i), new TextureChannelInput {
								enabled = true,
								invert = channelSource.Invert,
								output = ChannelIdxToEnum(i),
							});
							break;
						case TextureChannelPropertyValue._TYPE:
							var textureChannelProperty = (TextureChannelPropertyValue)channelSource.Source;
							input.texture = textureChannelProperty.Texture;
							input.SetChannelInput(ChannelIdxToEnum(textureChannelProperty.Channel), new TextureChannelInput {
								enabled = true,
								invert = channelSource.Invert,
								output = ChannelIdxToEnum(i),
							});
							break;
						default: throw new Exception("Unsupported PropertyValue Type");
						// also handle color and other property types
					}
					packer.Add(input);
				}
			}
			return packer.Create();
		}
	}
}
