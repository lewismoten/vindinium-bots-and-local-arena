﻿using Vindinium.Common.Entities;

namespace Vindinium.Game.Logic.Tests
{
    public class MockApiResponse : IApiResponse
    {
        public string ErrorMessage { get; set; }
        public string Text { get; set; }
        public bool HasError { get; set; }
    }
}