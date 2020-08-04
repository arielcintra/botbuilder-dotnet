﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses LGFile. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : LanguageGenerator
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TemplateEngineLanguageGenerator";

        private const string DEFAULTLABEL = "Unknown";

        private readonly LanguageGeneration.Templates lg;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        public TemplateEngineLanguageGenerator()
        {
            this.lg = new LanguageGeneration.Templates();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="engine">template engine.</param>
        public TemplateEngineLanguageGenerator(LanguageGeneration.Templates engine = null)
        {
            this.lg = engine ?? new LanguageGeneration.Templates();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="lgText">lg template text.</param>
        /// <param name="id">optional label for the source of the templates (used for labeling source of template errors).</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        [Obsolete("This method will soon be deprecated. Use Resource as the first parameter instead.")]
        public TemplateEngineLanguageGenerator(string lgText, string id, Dictionary<string, IList<Resource>> resourceMapping)
        {
            this.Id = id ?? DEFAULTLABEL;
            var (_, locale) = LGResourceLoader.ParseLGFileName(id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var lgResource = new LGResource(Id, Id, lgText ?? string.Empty);
            this.lg = LanguageGeneration.Templates.ParseResource(lgResource, importResolver);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="filePath">lg template file absolute path.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        [Obsolete("This method will soon be deprecated. Use Resource as the first parameter instead.")]
        public TemplateEngineLanguageGenerator(string filePath, Dictionary<string, IList<Resource>> resourceMapping)
        {
            filePath = PathUtils.NormalizePath(filePath);
            this.Id = Path.GetFileName(filePath);

            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var resource = new LGResource(Id, filePath, File.ReadAllText(filePath));
            this.lg = LanguageGeneration.Templates.ParseResource(resource, importResolver);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(Resource resource, Dictionary<string, IList<Resource>> resourceMapping)
        {
            this.Id = resource.Id;

            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var content = resource.ReadTextAsync().GetAwaiter().GetResult();
            var lgResource = new LGResource(Id, resource.FullName, content);
            this.lg = LanguageGeneration.Templates.ParseResource(lgResource, importResolver);
        }

        /// <summary>
        /// Gets or sets id of the source of this template (used for labeling errors).
        /// </summary>
        /// <value>
        /// Id of the source of this template (used for labeling errors).
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Method to generate text from given template and data.
        /// </summary>
        /// <param name="dialogContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>generated text.</returns>
        public override Task<object> GenerateAsync(DialogContext dialogContext, string template, object data, CancellationToken cancellationToken = default)
        {
            try
            {
                return Task.FromResult(lg.EvaluateText(template, data));
            }
            catch (Exception err)
            {
                if (!string.IsNullOrEmpty(this.Id))
                {
                    throw new Exception($"{Id}:{err.Message}");
                }

                throw;
            }
        }
    }
}
