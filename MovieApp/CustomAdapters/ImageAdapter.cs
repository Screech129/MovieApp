using System;
using Android.Widget;
using Android.Content;
using Android.Views;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Threading.Tasks;
using Org.Json;
using System.Collections.Generic;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using Square.Picasso;

namespace MovieApp
{
    public class ImageAdapter : BaseAdapter
    {
        Context context;
        ImageView imageView;
        List<string> posterPaths;
        List<int> movieIds;
        public ImageAdapter(Context c,List<string> p,List<int>i)
        {
            context = c;
            posterPaths = p;
            movieIds = i;
        }


        public void clear(){
            posterPaths.Clear();
        }

        public override int Count
        {
            get
            {
                return posterPaths.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }
            
        public override long GetItemId(int position)
        {
            return movieIds[position];
        }

        // create a new ImageView for each item
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
           

            if(convertView == null){
            //if it's not recycled, initialize some attributes
                imageView = new ImageView(context);
                imageView.LayoutParameters = new GridView.LayoutParams(GridView.LayoutParams.WrapContent,GridView.LayoutParams.WrapContent);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
                imageView.SetPadding(8,8,8,8);
              
            
            }else{
                imageView=(ImageView)convertView;
            }

            Picasso.With(context).Load("http://image.tmdb.org/t/p/w185/"+posterPaths[position]).Into(imageView);

            return imageView;
        }
         


    }
}

