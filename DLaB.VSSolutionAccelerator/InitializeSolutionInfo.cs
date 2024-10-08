﻿using DLaB.VSSolutionAccelerator.Wizard;
using System;
using System.Collections.Generic;
using System.IO;

namespace DLaB.VSSolutionAccelerator
{
    public class InitializeSolutionInfo : SolutionEditorInfo
    {
        public bool CreateSolution { get; set; }
        // public NuGetPackage XrmPackage { get; set; }
        public bool ConfigureEarlyBound { get; set; }
        public bool ConfigureXrmUnitTest { get; set; }
        public string RootNamespace { get; set; }
        //public override Version XrmVersion => XrmPackage.Version;

        private struct Page
        {
            public const int SolutionPath = 0;
            public const int RootNamespace = 1;
            public const int EarlyBound = 2;
            public const int CommonName = 3;
            public const int CommonWorkflowName = 4;
            public const int UseXrmUnitTest = 5;
            public const int CreatePlugin = 6;
            public const int PluginAssembly = 7;
            public const int PluginTest = 8;
            public const int CreateWorkflow = 9;
            public const int WorkflowTest = 10;
        }

        public static List<IWizardPage> InitializePages(List<KeyValuePair<int, string>> solutionNames)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var pages = new List<IWizardPage>();
            AddSolutionNameQuestion(pages); // 0
            AddRootNamespaceQuestion(pages);
            // AddXrmNuGetVersionQuestion(pages);
            AddUseEarlyBoundQuestion(pages);
            AddSharedCommonNameQuestion(pages);
            AddSharedCommonWorkflowNameQuestion(pages); 
            AddUseXrmUnitTestQuestion(pages); // 5
            AddCreatePluginProjectQuestion(pages);
            AddPluginAssemblyInfoQuestions(pages, solutionNames);
            AddPluginTestProjectNameQuestion(pages);
            AddCreateWorkflowProjectQuestion(pages);
            AddWorkflowTestProjectNameQuestion(pages); // 10
            AddInstallCodeSnippetAndAddCodeGenerationQuestion(pages);
            return pages;
        }

        private static void AddSolutionNameQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new ConditionalYesNoQuestionInfo("Do you want to add the DLaB Accelerators to an existing solution?")
            {
                Yes = new PathQuestionInfo("What Solution?")
                {
                    Filter = "Solution Files (*.sln)|*.sln",
                    Description = "This Wizard will walk through the process of adding isolation/plugin/workflow/testing projects based on the DLaB/XrmUnitTest framework.  The configured projects will be add to the solution defined here."
                },
                No = new PathQuestionInfo("Please enter the solution path to create.")
                {
                    Filter = "Folder",
                    Description = "This Wizard will walk through the process of adding isolation/plugin/workflow/testing projects based on the DLaB/XrmUnitTest framework.  The configured projects will be add to a new solution created at the path defined here.",
                    RequireFileExists = false,
                    DefaultResponse = "C:\\FolderUnderSourceControl\\YourCompanyAbbreviation.Dataverse.sln"
                },
            }));
        }

        private static void AddRootNamespaceQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new TextQuestionInfo("What is the root NameSpace?")
            {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.SolutionPath,1),
                EditDefaultResponse = (value) =>
                {
                    value = System.IO.Path.GetFileNameWithoutExtension(value) ?? "MyCompanyAbrv.Dataverse";
                    if (!value.ToLower().Contains("dataverse"))
                    {
                        value += ".Dataverse";
                    }
                    return value;
                },
                Description = "This is the root namespace that the Plugin and (if desired) Early Bound Entities will be appended to."
            }));
        }

        //private static void AddXrmNuGetVersionQuestion(List<IWizardPage> pages)
        //{
        //    pages.Add(NuGetVersionSelectorPage.Create(
        //        "What version of the SDK?",
        //        PackageLister.Ids.CoreXrmAssemblies,
        //        "This will determine the NuGet packages referenced and the version of the .Net Framework to use."));
        //}

        private static void AddUseEarlyBoundQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new ConditionalYesNoQuestionInfo("Do you want to use the Early Bound Generator To Create Early Bound Entities?")
            {
                Description = "If yes, generates the default Early Bound Generator Settings, but Configures the output paths of the entities/option sets/actions to generate files in the appropriate shared project within the solution."
                              + Environment.NewLine
                              + "The Early Bound Generator XrmToolBox plugin must be installed to generate the entities, and must be a version 1.2019.3.12 or greater."
                              + Environment.NewLine
                              + "The Early Bound Generator will also be triggered upon completion to generate the Early Bound classes."
            }));
        }

        private static void AddSharedCommonNameQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new TextQuestionInfo("What do you want the name of the shared common assembly to be?")
            {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace),
                Description = "This will be the name of a shared C# project that will be used to store common code including plugin logic and early bound entities if applicable"
            }));
        }

        private static void AddSharedCommonWorkflowNameQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new TextQuestionInfo("What do you want the name of the shared common workflow assembly to be?")
            {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace) + ".WorkflowCore",
                Description = "This will be the name of a shared C# project that contains references to the workflow code.  It would only be required by assemblies containing a workflow."
            }));
        }

        private static void AddUseXrmUnitTestQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new ConditionalYesNoQuestionInfo("Do you want to use XrmUnitTest for unit testing?")
            {
                Yes = new TextQuestionInfo("What do you want the Test Settings project to be called?")
                {
                    DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace) + ".Test",
                    Description = "The shared Test Project will contain all other shared test code (Assumption Definitions/Xml, Builders, Test Base Class, etc) and the single test settings config file."
                },
                Description = "This create the appropriate isolation projects."
            }));
        }

        private static void AddCreatePluginProjectQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new ConditionalYesNoQuestionInfo("Do you want to create a Plugin Project?")
            {
                Yes = new TextQuestionInfo("What should the plugin project be called?")
                {
                    DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace) + ".Plugin",
                    Description = "The name and default namespace for the plugin project."
                },
                Yes2 = new ComboQuestionInfo("Include example plugin classes?")
                {
                    DefaultResponse = 0,
                    Description = "If example plugin classes are included, it may contain compiler errors if the Early Bound Entities used in the files are not generated.",
                },
                Description = "This will add a new plugin project to the solution and wire up the appropriate references."
            }));
        }

        private static void AddPluginAssemblyInfoQuestions(List<IWizardPage> pages, List<KeyValuePair<int, string>> solutionNames)
        {
            var page = GenericPage.Create(new TextQuestionInfo("What is the name of the Company to use with the Plugin Assembly?")
            {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace),
                Description = "This information will be used in the plugin project file and will be used when generating the plugin assembly.",
                EditDefaultResponse = GetCompanyName
            }, new TextQuestionInfo("Plugin Assembly Description?") { 
                DefaultResponse = "Plugin with Dependent Assemblies",
            }, new ComboQuestionInfo("Solution to Deploy the Plugin to?")
            {
                Description = "The prefix of the solution publisher will be used to name the plugin package as well as upload a temporary plugin to for setting up a dev deployment build.",
                Options = solutionNames
            }, new TextQuestionInfo("Deployment PAC CLI Auth Name?") {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace),
                Description = "The PAC CLI Auth Name is the name of the used to PAC Auth context to use when deploying the plugin to dev.",
                EditDefaultResponse = (value) =>
                {
                    var companyName = GetCompanyName(value);
                    return string.IsNullOrWhiteSpace(companyName) ? "Dev" : companyName.Replace(" ", string.Empty) + " Dev";
                }
            });

            page.AddSavedValuedRequiredCondition(Page.CreatePlugin, "Y");
            pages.Add(page);
            return;

            string GetCompanyName(string value)
            {
                value = value.Replace("Xrm", string.Empty)
                    .Replace("Dataverse", string.Empty);
                value = string.Join(" ", value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
                value = string.Join(" ", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                return value;
            }
        }

        private static void AddPluginTestProjectNameQuestion(List<IWizardPage> pages)
        {
            var page = GenericPage.Create(new TextQuestionInfo("What do you want the name of the plugin test project to be?")
            {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.CreatePlugin, 1) + ".Tests",
                Description = "This will be the name of the Visual Studio Unit Test Project for the plugin assembly."
            });
            page.AddSavedValuedRequiredCondition(Page.CreatePlugin, "Y");
            page.AddSavedValuedRequiredCondition(Page.UseXrmUnitTest, "Y");
            pages.Add(page);
        }

        private static void AddCreateWorkflowProjectQuestion(List<IWizardPage> pages)
        {
            pages.Add(GenericPage.Create(new ConditionalYesNoQuestionInfo("Do you want to create a Workflow Project?")
            {
                Yes = new TextQuestionInfo("What should the workflow project be called??")
                {
                    DefaultResponse = GenericPage.GetSaveResultsFormat(Page.RootNamespace) + ".Workflow",
                    Description = "The name and default namespace for the workflow project."
                },
                Yes2 = new ComboQuestionInfo("Include example custom workflow activity?")
                {
                    DefaultResponse = 0
                },
                Description = "This will add a new workflow project to the solution and wire up the appropriate references."
            }));
        }

        private static void AddWorkflowTestProjectNameQuestion(List<IWizardPage> pages)
        {
            var page = GenericPage.Create(new TextQuestionInfo("What do you want the name of the workflow test project to be?")
            {
                DefaultResponse = GenericPage.GetSaveResultsFormat(Page.CreateWorkflow, 1) + ".Tests",
                Description = "This will be the name of the Visual Studio Unit Test Project for the workflow assembly."
            });
            page.AddSavedValuedRequiredCondition(Page.CreateWorkflow, "Y");
            page.AddSavedValuedRequiredCondition(Page.UseXrmUnitTest, "Y");
            pages.Add(page);
        }

        private static void AddInstallCodeSnippetAndAddCodeGenerationQuestion(List<IWizardPage> pages)
        {
            var page = GenericPage.Create(new ComboQuestionInfo("Do you want to install code snippets for plugins and unit testing?")
                {
                    DefaultResponse = 0,
                    Description = "This will install snippets in your local Visual Studio \"My Code Snippets\" directories."
                },
                new ComboQuestionInfo("Do you want to add Code Generation files to your solution?")
                {
                    DefaultResponse = 0,
                    Description = "The snippet files installed and a LinqPad Guid Generator file will be added to the solution for reference."
                });
            pages.Add(page);
        }

        private InitializeSolutionInfo(Queue<object> queue, Dictionary<int, Guid> solutionIdsByIndex)
        {
            InitializeSolution(new YesNoResult(queue.Dequeue())); // 0
            RootNamespace = (string)queue.Dequeue();
            //XrmPackage = (NuGetPackage)queue.Dequeue();
            ConfigureEarlyBound = queue.Dequeue().ToString() == "Y";
            SharedCommonProject = (string)queue.Dequeue();
            SharedCommonWorkflowProject = (string)queue.Dequeue(); 
            InitializeXrmUnitTest(new YesNoResult(queue.Dequeue())); // 5
            InitializePlugin(new YesNoResult(queue.Dequeue()));
            InitializePluginAssembly((List<string>) queue.Dequeue(), solutionIdsByIndex);
            PluginTestName = (string)queue.Dequeue();
            InitializeWorkflow(new YesNoResult(queue.Dequeue()));
            WorkflowTestName = (string)queue.Dequeue(); 
            InitializeCodeGeneration((List<string>)queue.Dequeue()); // 10

            if (ConfigureXrmUnitTest)
            {
                CreatePluginTest = CreatePlugin;
                CreateWorkflowTest = CreateWorkflow;
            }
        }

        public static InitializeSolutionInfo InitializeSolution(object[] values, Dictionary<int, Guid> solutionIdsByIndex)
        {
            return new InitializeSolutionInfo(new Queue<object>(values), solutionIdsByIndex);
        }

        public string GetEarlyBoundSettingsPath()
        {
            var settingsDirectory = Path.Combine(Path.GetDirectoryName(SolutionPath) ?? "", SharedCommonProject, "Entities");
            Directory.CreateDirectory(settingsDirectory);
            return Path.Combine(settingsDirectory, "DLB.EarlyBoundGenerator.Settings.xml");
        }

        private void InitializeSolution(YesNoResult result)
        {
            CreateSolution = !result.IsYes;
            SolutionPath =  result[1];
        }

        private void InitializeXrmUnitTest(YesNoResult result)
        {
            ConfigureXrmUnitTest = result.IsYes;
            TestBaseProject = result[1];
            SharedTestCoreProject = result[2];
        }

        private void InitializePlugin(YesNoResult result)
        {
            CreatePlugin = result.IsYes;
            PluginName = result[1];
            IncludeExamplePlugins = result[2] == "0";
        }

        private void InitializePluginAssembly(List<string> result, Dictionary<int, Guid> solutionIdsByIndex)
        {
            PluginPackage.Company = result[0];
            PluginPackage.Description = result[1];
            PluginPackage.SolutionId = solutionIdsByIndex[int.Parse(result[2])];
            PluginPackage.PacAuthName = result[3];
        }

        private void InitializeWorkflow(YesNoResult result)
        {
            CreateWorkflow = result.IsYes;
            WorkflowName = result[1];
            IncludeExampleWorkflow = result[2] == "0";
        }

        private void InitializeCodeGeneration(List<string> result)
        {
            InstallSnippets = result[0] == "0";
            IncludeCodeGenerationFiles = result[1] == "0";
        }
    }
}
