﻿using EasyOC;
using EasyOC.OrchardCore.RDBMS.Models;
using EasyOC.OrchardCore.RDBMS.Services;
using EasyOC.OrchardCore.RDBMS.ViewModels;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.RelationDb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly ISession _session;
        private readonly IContentFieldsValuePathProvider _contentFieldsValuePathProvider;



        public AdminController(
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IUpdateModelAccessor updateModelAccessor,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager, ISession session, IContentFieldsValuePathProvider contentFieldsValuePathProvider)
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _contentFieldsValuePathProvider = contentFieldsValuePathProvider;
        }

        [ActionName("CreateOrEditAsync")]
        public async Task<IActionResult> CreateOrEditPost(RDBMSMappingConfigViewModel model)
        {
            ContentItem contentItem;
            if (string.IsNullOrEmpty(model.Id))
            {
                contentItem = await _contentManager.NewAsync("RDBMSMappingConfig");
            }
            else
            {
                contentItem = await _contentManager.GetAsync(model.Id);
            }
            var dbEntity = contentItem.As<RDBMSMappingConfig>();

            //     var contentItem = new ContentItem()
            //     {
            //         ContentType = "RDBMSMappingConfig",
            //         DisplayText=model.ConfigName, 
            // };
            return View(model);
        }

        public IActionResult CreateOrEdit()
        {
            var model = new RDBMSMappingConfigViewModel();
            return View(model);
        }

        public async Task<RecipeModel> GenerateRecipeAsync(string tableName, string connectionConfigId)
        {

            var connectionObject = await _contentManager.GetAsync(connectionConfigId);
            IFreeSql freeSql = FreeSqlProviderFactory.GetFreeSql(connectionObject.Content.DbConnectionConfig.ProviderName.Text.Value, connectionObject.Content.DbConnectionConfig.ConnectionString.Text.Value);
            using (freeSql)
            {
                var recipe = new RecipeModel();

                var step = new Step();
                recipe.steps = new List<Step>() { step };
                step.name = "ContentDefinition";
                step.ContentTypes = new List<ContentType>();
                var contentType = new ContentType()
                {
                    Name = tableName,
                    DisplayName = tableName,
                    Settings = JObject.Parse(@"
                                            {'ContentTypeSettings': {
                                                'Creatable': true,
                                                  'Listable': true,
                                                  'Draftable': true,
                                                  'Versionable': true,
                                                  'Securable': true
                                            }}")


                };
                step.ContentTypes.Add(contentType);
                contentType.ContentTypePartDefinitionRecords = new ContentTypePartDefinitionRecord[]{ new ContentTypePartDefinitionRecord
                    {
                        Name = tableName,
                        PartName =tableName
                    }};

                var recrods = new List<ContentPartFieldDefinitionRecord>();
                try
                {

                    var tb = freeSql.Select<object>().AsTable((type, oldname) => tableName).First();
                    var Tbs = freeSql.DbFirst.GetTablesByDatabase();
                    foreach (var item in tb.GetType().GetProperties())
                    {
                        var recrod = new ContentPartFieldDefinitionRecord();
                        recrod.Name = item.Name;
                        recrod.Settings = JObject.FromObject(new
                        {
                            ContentPartFieldSettings = new { DisplayName = item.Name }
                        });
                        var targetFieldType = _contentFieldsValuePathProvider.GetField(item.PropertyType);
                        recrod.FieldName = targetFieldType.FieldName;
                        recrods.Add(recrod);
                    }

                    step.ContentParts.Add(new Contentpart { Name = tableName, ContentPartFieldDefinitionRecords = recrods.ToArray() });

                    return recipe;
                }
                catch (System.Exception)
                {
                    return null;
                }
            }

        }

        public async Task<IEnumerable<SelectListItem>> GetAllDbConnecton()
        {
            var connectionSettings = await _session.Query<ContentItem, ContentItemIndex>()
                                           .Where(x => x.ContentType == "DbConnectionConfig" && (x.Published || x.Latest)).ListAsync();
            var connectionList = connectionSettings.Select(x => new SelectListItem() { Text = S[x.DisplayText], Value = x.ContentItemId });
            return connectionList;
        }

        public IEnumerable<SelectListItem> GetAllTypes()
        {
            var allTypes = _contentDefinitionManager.ListTypeDefinitions();
            var contentTypeslist = allTypes.Select(x => new SelectListItem() { Text = S[x.DisplayName], Value = x.Name });
            return contentTypeslist;
        }


        /// <summary>
        /// Get
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetAllConnectionList()
        {
            var connectionSettings = await _session.Query<ContentItem, ContentItemIndex>()
                  .Where(x =>
                      x.ContentType == "DbConnectionConfig" &&
                      (x.Published || x.Latest)).ListAsync();
            return Json(connectionSettings);
        }

        public async Task<object> TryGetFileds()
        {
            var contentItems = await _session.Query<ContentItem, ContentItemIndex>()
                   .Where(x =>
                       x.ContentType == "RelationalDbMapping" &&
                       (x.Published || x.Latest)).ListAsync();

            return null;
        }


        public IActionResult GenerateMappingData(string typeName)
        {
            var type = _contentDefinitionManager.LoadTypeDefinition(typeName);

            var part = type.Parts.FirstOrDefault(x => x.Name == type.Name);
            var partName = part.Name;
            var partFileds = new List<object>();
            // This builder only handles parts with fields.
            foreach (var field in part.PartDefinition.Fields)
            {
                var fieldType = _contentFieldsValuePathProvider.GetField(field);
                if (fieldType != null)
                {
                    partFileds.Add(new
                    {
                        name = field.Name,
                        ocFieldType = field.FieldDefinition.Name,
                        valuePath = $"{type.Name}.{field.Name}.{fieldType.ValuePath}",
                        dbField = field.Name
                    });
                }
            }
            return Json(JArray.FromObject(partFileds).ToString());

        }


        public IActionResult Index()
        {
            return View();
        }
    }
}



