using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ImageRecognitionWrapper;
using System.Drawing;

namespace CustomImageTagger
{
    public class ImageTaggerProcessor : IProcessor
    {
        private ImageTagsGenerator tagger;
        private string imgColName;
        private string numColName;
        private string tagColName;

        public ImageTaggerProcessor(string imgCol = "ImgData", string numCol = "NumObjects", string tagCol = "Tags")
        {
            this.tagger = new ImageTagsGenerator();
            this.numColName = numCol;
            this.imgColName = imgCol;
            this.tagColName = tagCol;
        }

        public override IRow Process(IRow input, IUpdatableRow output)
        {

            var tags = this.tagger.ProduceTags(input.Get<byte[]>(this.imgColName));

            var stringOfTags = string.Join(";", tags.Select(x => string.Format("{0}:{1}", x.Key, x.Value)));

            output.Set<int>(this.numColName, tags.Count);
            output.Set<string>(this.tagColName, stringOfTags);

            return output.AsReadOnly();
        }

        private class ImageTagsGenerator
        {
            private Recognizer recognizer;
            private float _threshold;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="threshold">Specifies the threshold of which tags to include. DEfault set to 0.5</param>
            public ImageTagsGenerator(float threshold = 0.5f)
            {

                string currentDirectory = Directory.GetCurrentDirectory();
                this.recognizer = new Recognizer();
                this.recognizer.Initialize(currentDirectory, Environment.ProcessorCount);
                this.recognizer.AddCategoryInfo(currentDirectory + "\\category_threshold_coco.txt", "");
                this.recognizer.AddCategoryInfo(currentDirectory + "\\category_threshold_imagenet.txt", "");

                this._threshold = threshold;
            }

            public Dictionary<string, float> ProduceTags(byte[] imgData)
            {
                var results = new Dictionary<string, float>();

                this.recognizer.Compute(new Bitmap((Stream)new MemoryStream(imgData)));

                List<List<RecognizationResult>> recognizationResultListList = new List<List<RecognizationResult>>();
            
                for (int index = 0; index < this.recognizer.GetNumOfClassifiers(); ++index)
                    recognizationResultListList.Add(this.recognizer.Classify(index));

                foreach(var listOfTags in recognizationResultListList)
                {
                    foreach(var result in listOfTags)
                    {
                        if (!results.ContainsKey(result.name) && result.confidence > _threshold)
                        {
                            results.Add(result.name, result.confidence);
                        }
                    }
                }

                return results;
            }
        }
    }
}