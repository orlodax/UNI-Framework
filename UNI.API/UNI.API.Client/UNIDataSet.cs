﻿using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;

namespace UNI.API.Client;

public class UNIDataSet<T> : UniDataSet<T> where T : BaseModel
{
    override public async Task<List<T>> Get(int? id = null, string? idName = null, int? requestedEntriesNumber = 50, int blockToReturn = 1, string? filterText = null, bool skipInit = false, List<FilterExpression>? filterExpressions = null)
    {
        var request = new GetDataSetRequestDTO()
        {
            Id = id,
            IdName = idName,
            RequestedEntriesNumber = requestedEntriesNumber,
            BlockToReturn = blockToReturn,
            FilterText = filterText,
            FilterExpressions = filterExpressions,
            SkipInit = skipInit,
        };

        ApiResponseModel<T>? response = await new UNIClient<T>().GetDataSet(request);

        if (response == null)
            return new List<T>();

        Count = response.Count;
        DataBlocks = response.DataBlocks;
        return response.ResponseBaseModels;
    }

    override public object Query(string query)
    {
        throw new NotImplementedException();
    }
}
