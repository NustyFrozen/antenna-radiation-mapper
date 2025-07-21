using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PolarizationMapper.Models;

public class CoordinatesToCsv
{
    public static string CreateCsvfile(string filename,List<polar> datapoints,Tuple<double, double>? range,double scalePx)
    {
        string text = "angle,mag";
        datapoints = datapoints.OrderBy(x => x.angle).ToList();
        polar[] flip = datapoints.ToArray();
        //shifting 180 degrees
        for (int i = 0; i < 360; i++)
        {
            flip[i].angle = (Convert.ToInt32(flip[i].angle + 180)) % 360;
        }
        datapoints = flip.ToList();
        datapoints = datapoints.OrderBy(x => x.angle).ToList();
        foreach (polar currentPoint in datapoints)
        {
            var scaledAngle = currentPoint.angle;
            var scaledData = (currentPoint.magnitude / scalePx * (range!.Item2 - range.Item1)) + range.Item1;
            text += $"\n{currentPoint.angle},{scaledData}";
        }

        var file = new FileInfo(filename);
        var resultsPath = Path.Combine(file.Directory!.FullName, file.Name) + ".csv";
        File.WriteAllText(resultsPath, text);
        return resultsPath;
    }
}