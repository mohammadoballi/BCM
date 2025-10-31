using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Base.DTOs
{
    public class DefaultResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message_en { get; set; } = string.Empty;
        public string Message_ar { get; set; } = string.Empty;
        public T? Data { get; set; }

        public Pagination Pagination { get; set; } = null;

        DefaultResponse()
        {
            IsSuccess = false;
            Message_en = string.Empty;
            Message_ar = string.Empty;
            Pagination = null;
        }

        public static DefaultResponse<T> SuccessResponse(T data, string message_en = "", string message_ar = "", Pagination pagination = null)
        {
            return new DefaultResponse<T>
            {
                IsSuccess = true,
                Message_en = message_en,
                Message_ar = message_ar,
                Data = data,
                Pagination = pagination

            };
        }

        public static DefaultResponse<T> FailureResponse(string message_en = "", string message_ar = "")
        {
            return new DefaultResponse<T>
            {
                IsSuccess = false,
                Message_en = message_en,
                Message_ar = message_ar,
                Pagination = null
            };
        }

    }

    public class Pagination
    {
        public int Total { get; set; }
        public int Index { get; set; }
        public int Size { get; set; }
        public int TotalPages { get; set; }
    }
}
