﻿using System;

namespace Transformers.BasicTransformersType
{
    [Obsolete]
    public class TransformerStringInt : TransformerChangeType<string, int>
    {
        public TransformerStringInt(string oldField, string newField)
            : base(oldField, newField)
        {
        }
    }
}
