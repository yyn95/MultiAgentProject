using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Rectangle
{
    public int xmax, ymax, xmin, ymin;
    public int xmid, ymid;
    public Rectangle(int x, int y)
    {
        xmax = xmin = x;
        ymax = ymin = y;
        xmid = x;
        ymid = y;
    }
    public void setMid()
    {
        xmid = xmin + (xmax - xmin) / 2;
        ymid = ymin + (ymax - ymin) / 2;
    }

}
