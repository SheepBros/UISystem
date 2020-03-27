using System;

namespace SB
{
    public class UIAsset : IEquatable<UIAsset>
    {
        public string Bundle { get; private set; }

        public string Name { get; private set; }

        public UIAsset(string bundle, string name)
        {
            Bundle = bundle;
            Name = name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null && obj.GetType() != typeof(UIAsset))
            {
                return false;
            }

            return Equals((UIAsset) obj);
        }

        public bool Equals(UIAsset asset)
        {
            return Bundle == asset.Bundle && Name == asset.Name;
        }

        public static bool operator ==(UIAsset asset1, UIAsset asset2)
        {
            return asset1.Equals(asset2);
        }

        public static bool operator !=(UIAsset asset1, UIAsset asset2)
        {
            return !asset1.Equals(asset2);
        }
    }
}