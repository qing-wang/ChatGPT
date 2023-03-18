using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatGPT.Model.Plugins;
using ChatGPT.Model.Services;
using ChatGPT.ViewModels.Chat;
using ChatGPT.ViewModels.Layouts;
using ChatGPT.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace ChatGPT.ViewModels;

public partial class MainViewModel : ObservableObject, IPluginContext
{
    private static readonly MainViewModelJsonContext s_serializerContext = new(
        new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            IncludeFields = false,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        });

    public MainViewModel()
    {
        _chats = new ObservableCollection<ChatViewModel>();
        _prompts = new ObservableCollection<PromptViewModel>();

        SingleLayout = new SingleLayoutViewModel();

        ColumnLayout = new ColumnLayoutViewModel();

        _layouts = new ObservableCollection<LayoutViewModel>
        {
            SingleLayout,
            ColumnLayout
        };

        CurrentLayout = SingleLayout;

        Layout = "Mobile";

        InitPromptCallback();

        NewChatCallback();

        // Chats
        
        AddChatCommand = new AsyncRelayCommand(AddChatAction);

        DeleteChatCommand = new AsyncRelayCommand(DeleteChatAction);

        OpenChatCommand = new AsyncRelayCommand(OpenChatAction);

        SaveChatCommand = new AsyncRelayCommand(SaveChatAction);

        ExportChatCommand = new AsyncRelayCommand(ExportChatAction);

        CopyChatCommand = new AsyncRelayCommand(CopyChatAction);

        DefaultChatSettingsCommand = new RelayCommand(DefaultChatSettingsAction);

        // Prompts

        AddPromptCommand = new AsyncRelayCommand(AddPromptAction);

        DeletePromptCommand = new AsyncRelayCommand(DeletePromptAction);

        OpenPromptsCommand = new AsyncRelayCommand(OpenPromptsAction);

        SavePromptsCommand = new AsyncRelayCommand(SavePromptsAction);

        ImportPromptsCommand = new AsyncRelayCommand(ImportPromptsAction);

        CopyPromptCommand = new AsyncRelayCommand(CopyPromptAction);

        SetPromptCommand = new AsyncRelayCommand(SetPromptAction);

        // Actions
    
        ExitCommand = new RelayCommand(ExitAction);

        ChangeThemeCommand = new RelayCommand(ChangeThemeAction);

        ChangeDesktopMobileCommand = new RelayCommand(ChangeDesktopMobileAction);
    }

    public async Task LoadSettings()
    {
        var factory = Ioc.Default.GetService<IStorageFactory>();
        var storage = factory?.CreateStorageService<WorkspaceViewModel>();
        if (storage is null)
        {
            return;
        }
        var workspace = await storage.LoadObject("Settings", s_serializerContext.WorkspaceViewModel);
        if (workspace is { })
        {
            if (workspace.Chats is { })
            {
                foreach (var chat in workspace.Chats)
                {
                    foreach (var message in chat.Messages)
                    {
                        chat.SetMessageActions(message);
                    }
                }

                Chats = workspace.Chats;
                CurrentChat = workspace.CurrentChat;
            }

            if (workspace.Prompts is { })
            {
                Prompts = workspace.Prompts;
                CurrentPrompt = workspace.CurrentPrompt;
            }

            if (workspace.Layouts is { })
            {
                Layouts = workspace.Layouts;
                CurrentLayout = workspace.CurrentLayout;
                SingleLayout = Layouts.OfType<SingleLayoutViewModel>().FirstOrDefault();
                ColumnLayout = Layouts.OfType<ColumnLayoutViewModel>().FirstOrDefault();
            }

            if (workspace.Layout is { })
            {
                Layout = workspace.Layout;
            }            

            if (workspace.Theme is { })
            {
                Theme = workspace.Theme;
            }
        }
    }

    public async Task SaveSettings()
    {
        var workspace = new WorkspaceViewModel
        {
            Chats = Chats,
            CurrentChat = CurrentChat,
            Prompts = Prompts,
            CurrentPrompt = CurrentPrompt,
            Layouts = Layouts,
            CurrentLayout = CurrentLayout,
            Theme = Theme,
            Layout = Layout,
        };

        var factory = Ioc.Default.GetService<IStorageFactory>();
        var storage = factory?.CreateStorageService<WorkspaceViewModel>();
        if (storage is { })
        {
            await storage.SaveObject(workspace, "Settings", s_serializerContext.WorkspaceViewModel);
        }
    }
}
