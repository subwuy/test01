
// class def
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassDef
{

    /*
     notes - 
          if link.loc >= target lane
          then divert ( update railid, send msg to db
          move hook to new rail and let link continue
          
          update location - move everthing one step pulse
    
      4 ms scan rate
      16 steps per scan cycel

      color meanings
      
      link
          white - AVL
          gray - blocked
          yellow used
    
      hook
          pink - no lpn
          orange - has lpn put no sku
          red - has lpn, sku , but no orderid
          green - lpn assigned to order
          blue -  assigend to pack
          brown - pack complete


        scanner	0	    0
        mary01	26227	26227
        mary02	5390	31617
        mary03	17121	48738
        mary04	4305	53043
        mary05	16594	69637
        mary06	4305	73942
        mary07	17114	91056
        mary08	5380	96436
        mary09	17290	113726
        mary10	4308	118034
       
     */



    public class clsRail
    {
        public float start_Loc_x; // this will be the center of width and edge of lenght 
        public float start_Loc_y;
        public int directionOfTravel; // used with enum direction

        public float width;
        public float lenght; // how long the rail is
        public int railID; // how to identify this rail
        public int railChild; // who this rail dumps into
        public int laneNumber; // used for dest request
        public int railType; // power rail or gravity, also break out cam vs non cam
        // 1 = sky train
        // 2 = gravity cam
        // 3 = gravity non cam

        // constructor
        public clsRail()
        {
            directionOfTravel = 1;
            railChild = 0;
            laneNumber = 0;
            railType = 1;
        }


        public enum direction
        {
            left = 1,
            right = 2,
            up = 3,
            down = 4,
        };
    }

    public class clsLink
    {

        public float center_loc_x;
        public float center_loc_y;
        public int color;
        // 1 = red
        // 2 = green
        // 3 = blue
        // 4 = gray
        // 5 = white
        // 6 = orange
        // 7 = pink
        // 8 = brown
        // 9 = yellow


        // link
        // white - AVL
        // gray - blocked
        // yellow used

        public int currentRail; // used to track teh current rail ID it attached to, this contols speed and direction
        public int size;

        // constructor
        public clsLink()
        {

        }

        public enum ColorStatus
        {
            available = 5,
            blocked = 4,
            occupied = 9,
        };
    }

    public class clsHook
    {

        public string LPN;
        public int LPN_RecID;
        public int color;
        public bool LE; //leading edge
        public bool Latched;
        public bool TE; //trailing edge


        // constructor
        public clsHook()
        {

        }

    }

}
// class def