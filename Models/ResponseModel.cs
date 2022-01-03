using System;
using System.Text.Json.Nodes;

namespace Druzhbank.Models
{
    public class ResponseModel<T>
    {
        public Boolean success { get; set; }
        public T? data { get; set; }
    }
}