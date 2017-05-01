using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FaceMaskExample
{
    public static class ExampleDataSet
    {
        private static int index = 0;
        public static int Index
        {
            get { return index; }
        }

        public static int Length
        {
            get { return filenames.Length; }
        }

        private static string[] filenames = new string[5]{
            "face_mask1",
            "face_mask2",
            "face_mask3",
            "face_mask4",
            "face_mask5"
        };

        private static Rect[] faceRcts = new Rect[5]{
            new Rect(),
            new Rect(),
            new Rect(),
            //panda
            new Rect(17, 64, 261, 205),
            //anime
            new Rect(56, 85, 190, 196)
        };

        private static List<Vector2>[] landmarkPoints = new List<Vector2>[5]{
            null,
            null,
            null,
            //panda
            new List<Vector2>(){
                new Vector2(31, 136),
                new Vector2(23, 169),
                new Vector2(26, 195),
                new Vector2(35, 216),
                new Vector2(53, 236),
                new Vector2(71, 251),
                new Vector2(96, 257),
                new Vector2(132, 259),
                new Vector2(143, 263),
                //9
                new Vector2(165, 258),
                new Vector2(198, 255),
                new Vector2(222, 242),
                new Vector2(235, 231),
                new Vector2(248, 215),
                new Vector2(260, 195),
                new Vector2(272, 171),
                new Vector2(264, 135),
                //17
                new Vector2(45, 115),
                new Vector2(70, 94),
                new Vector2(97, 89),
                new Vector2(116, 90),
                new Vector2(135, 105),
                new Vector2(157, 104),
                new Vector2(176, 90),
                new Vector2(198, 86),
                new Vector2(223, 90),
                new Vector2(248, 110),
                //27
                new Vector2(148, 134),
                new Vector2(147, 152),
                new Vector2(145, 174),
                new Vector2(144, 192),
                new Vector2(117, 205),
                new Vector2(128, 213),
                new Vector2(143, 216),
                new Vector2(160, 216),
                new Vector2(174, 206),
                //36
                new Vector2(96, 138),
                new Vector2(101, 131),
                new Vector2(111, 132),
                new Vector2(114, 140),
                new Vector2(109, 146),
                new Vector2(100, 146),
                new Vector2(180, 138),
                new Vector2(186, 130),
                new Vector2(195, 131),
                new Vector2(199, 137),
                new Vector2(195, 143),
                new Vector2(185, 143),
                //48
                new Vector2(109, 235),
                new Vector2(118, 231),
                new Vector2(129, 228),
                new Vector2(143, 225),
                new Vector2(156, 227),
                new Vector2(174, 232),
                new Vector2(181, 234),
                new Vector2(173, 241),
                new Vector2(156, 245),
                new Vector2(143, 245),
                new Vector2(130, 244),
                new Vector2(117, 239),
                new Vector2(114, 235),
                new Vector2(130, 232),
                new Vector2(142, 232),
                new Vector2(157, 233),
                new Vector2(175, 236),
                new Vector2(155, 237),
                new Vector2(143, 238),
                new Vector2(130, 237)
            },
            //anime
            new List<Vector2>(){
                new Vector2(62, 179),
                new Vector2(72, 209),
                new Vector2(75, 223),
                new Vector2(81, 236),
                new Vector2(90, 244),
                new Vector2(101, 251),
                new Vector2(116, 258),
                new Vector2(129, 262),
                new Vector2(142, 268),
                new Vector2(160, 265),
                new Vector2(184, 260),
                new Vector2(202, 253),
                new Vector2(210, 247),
                new Vector2(217, 239),
                new Vector2(222, 229),
                new Vector2(225, 222),
                new Vector2(243, 191),
                //17
                new Vector2(68, 136),
                new Vector2(86, 128),
                new Vector2(104, 126),
                new Vector2(122, 131),
                new Vector2(134, 141),
                new Vector2(177, 143),
                new Vector2(191, 135),
                new Vector2(209, 132),
                new Vector2(227, 136),
                new Vector2(239, 143),
                //27
                new Vector2(153, 163),
                new Vector2(150, 190),
                new Vector2(149, 201),
                new Vector2(148, 212),
                new Vector2(138, 217),
                new Vector2(141, 219),
                new Vector2(149, 221),
                new Vector2(152, 220),
                new Vector2(155, 217),
                //36
                new Vector2(70, 182),
                new Vector2(85, 165),
                new Vector2(114, 168),
                new Vector2(122, 192),
                new Vector2(113, 211),
                new Vector2(82, 209),
                new Vector2(177, 196),
                new Vector2(189, 174),
                new Vector2(220, 175),
                new Vector2(234, 192),
                new Vector2(215, 220),
                new Vector2(184, 217),
                //48
                new Vector2(132, 249),
                new Vector2(134, 249),
                new Vector2(139, 250),
                new Vector2(144, 251),
                new Vector2(148, 251),
                new Vector2(153, 250),
                new Vector2(155, 251),
                new Vector2(154, 253),
                new Vector2(149, 257),
                new Vector2(144, 257),
                new Vector2(138, 256),
                new Vector2(133, 252),
                new Vector2(133, 250),
                new Vector2(139, 252),
                new Vector2(144, 254),
                new Vector2(148, 253),
                new Vector2(153, 251),
                new Vector2(148, 254),
                new Vector2(144, 254),
                new Vector2(139, 253),
            }
        };

        public static ExampleMaskData GetData(){
            return new ExampleMaskData(filenames[index], faceRcts[index], landmarkPoints[index]);
        }

        public static ExampleMaskData GetData(int index){
            return new ExampleMaskData(filenames[index], faceRcts[index], landmarkPoints[index]);
        }

        public static void Next(){
            index++;
            if(index == filenames.Length)
                index = 0;
        }
    }

    public class ExampleMaskData
    {
        private string filename;
        public string FileName
        {
            get { return this.filename; }
        }

        private Rect faceRect;
        public Rect FaceRect
        {
            get { return this.faceRect; }
        }

        private List<Vector2> landmarkPoints;
        public List<Vector2> LandmarkPoints
        {
            get { return this.landmarkPoints; }
        }
        
        public ExampleMaskData(string filename, Rect faceRect, List<Vector2> landmarkPoints){
            this.filename = filename;
            this.faceRect = faceRect;
            this.landmarkPoints = landmarkPoints;
        }
    }
}