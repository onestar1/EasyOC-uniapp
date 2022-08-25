﻿using EasyOC.ContentExtensions.AppServices.Dtos;
using EasyOC.ContentExtensions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyOC.ContentExtensions.AppServices
{
    public interface IContentTypeManagementAppService
    {
        PagedResult<ContentPartDefinitionDto> GetAllParts(SimpleFilterAndPageQueryInput input);
        PagedResult<ContentTypeListItemDto> GetAllTypes(GetAllTypeFilterInput input);
        ContentPartDefinitionDto GetPartDefinition(string name, bool withSettings = false);
        ContentTypeDefinitionDto GetTypeDefinition(string name, bool withSettings = false);
        List<ContentFieldsMappingDto> GetFields(string typeName);
        Task<IEnumerable<QueryDefDto>> ListAllQueriesAsync();
    }
}