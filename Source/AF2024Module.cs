using System;
using System.Reflection;
using System.Threading;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.AF2024;

public class AF2024Module : EverestModule {
    public static AF2024Module Instance { get; private set; }

    public override Type SettingsType => typeof(AF2024ModuleSettings);
    public static AF2024ModuleSettings Settings => (AF2024ModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(AF2024ModuleSession);
    public static AF2024ModuleSession Session => (AF2024ModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(AF2024ModuleSaveData);
    public static AF2024ModuleSaveData SaveData => (AF2024ModuleSaveData) Instance._SaveData;

    private Hook? splashFinishHook;
    public Hook? splashUpdateHook;

    private static readonly string[] GameNames = [
        "Minecraft",
        "WII Sports",
        "Celeste Classic",
        "New Super Mario Bros U",
        "Mario Kart 8 deluxe",
        "Super Mario 3D World",
        "Portal",
        "Portal 2",
        "Geometry Dash",
        "Baba is you",
        "Tetris",
        "Super Mario Galaxy",
        "Super Mario Galaxy 2",
        "Rayman Legends",
        "Fornite",
        "Overwatch",
        "Undertale",
        "Grand Theft Auto V",
        "Super Meat Boy",
        "Team Fortress 2",
        "Quake II",
        "Pacman"
    ];

    private static readonly string[] StuffNames = [
        "Olympus",
        "Windows",
        "Linux",
        "MacOS",
        "Paint.net",
        "WinRar",
        "7zip",
        "Firefox",
        "Chrome",
        "Safari",
        "Discord",
        "Gamebanana",
        "Steam",
        "Itch",
        "RAM",
        "an epic image for you",
        "something",
        ".....................................................................",
        "fuel into the engine",
        "another splash window",
        "a new country",
        "a bob-omb",
        "your favorite mod",
        ":gladeline:",
    ];

    private static Random RNG;
    private static int extraMods = 0;

    private static Mode mode;
    private static FieldInfo amountLoadedField;
    private static FieldInfo totalToLoadField;
    private static int realTotalMods;


    public AF2024Module() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(AF2024Module), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(AF2024Module), LogLevel.Info);
#endif
        amountLoadedField = typeof(EverestSplashHandler).GetField("loadedMods", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw FieldBroke(nameof(EverestSplashHandler), "loadedMods");
        totalToLoadField = typeof(EverestSplashHandler).GetField("totalMods", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw FieldBroke(nameof(EverestSplashHandler), "totalMods");
    }

    public override void Load() {
        int seed = Convert.ToInt32(Environment.GetEnvironmentVariable("SPLASHFUNNY_SEED"));
        int forceMode = Convert.ToInt32(Environment.GetEnvironmentVariable("SPLASHFUNNY_MODE") ?? "-1");
        bool disableExtra = Environment.GetEnvironmentVariable("SPLASHFUNNY_DISABLEEXTRA") == "1";
        bool disableMad = Environment.GetEnvironmentVariable("SPLASHFUNNY_DISABLEMAD") == "1";
        
        RNG = seed != 0 ? new Random(seed) : new Random();
        Mode[] arr = (Mode[])Enum.GetValuesAsUnderlyingType<Mode>();
        if (forceMode == -1 || forceMode >= arr.Length - (disableMad ? -1 : 0))
            mode = arr[RNG.Next(arr.Length - (disableMad ? -1 : 0))];
        else
            mode = (Mode)forceMode;
        
        extraMods = disableExtra ? 0 : RNG.Next(10);
        
        splashFinishHook = new Hook(
            typeof(EverestSplashHandler).GetMethod(nameof(EverestSplashHandler.StopSplash)) 
                ?? throw MethodBroke(nameof(EverestSplashHandler), nameof(EverestSplashHandler.StopSplash)),
            Hook_StopSplash);
        splashUpdateHook =
            new Hook(
                typeof(EverestSplashHandler).GetMethod("UpdateSplashLoadingProgress",
                    BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(string) }) 
                    ?? typeof(EverestSplashHandler).GetMethod("SendMessageToSplash",
                    BindingFlags.Static | BindingFlags.NonPublic, new []{ typeof(string) })
                    ?? throw MethodBroke(nameof(EverestSplashHandler), "UpdateSplashLoadingProgress or SendMessageToSplash"),
                Hook_SplashUpdate);
        // Here the total to load value must have not been messed with (yet)
        realTotalMods = (int)(totalToLoadField.GetValue(null) 
            ?? throw NullBroke(nameof(realTotalMods)));
    }

    public override void Unload() {
        splashFinishHook?.Dispose();
        splashUpdateHook?.Dispose();
    }

    private static Exception ModBroke(Exception? inner = null) {
        return new NotSupportedException("This mod broke :saddleline:", inner);
    }
    private static Exception FieldBroke(string? className, string? fieldName) {
        return ModBroke(new MissingFieldException(className, fieldName));
    }
    private static Exception MethodBroke(string? className, string? methodName){
        return ModBroke(new MissingMethodException(className, methodName));
    }
    private static Exception NullBroke(string? paramName){
        return ModBroke(new ArgumentNullException(paramName));
    }

    private static void Hook_SplashUpdate(Action<string> orig, string name) {
        if (mode != Mode.Mad) {
            string chosen = mode switch {
                Mode.RandomGames => GameNames[RNG.Next(GameNames.Length)],
                Mode.RandomStuff => StuffNames[RNG.Next(StuffNames.Length)],
                Mode.Mad => throw new NotSupportedException(), // What
                _ => throw new NotSupportedException(),
            };
            
            orig(chosen);
        } else { // Heh
            amountLoadedField.SetValue(null, RNG.Next(1, realTotalMods + extraMods));
            orig(name);
        }
    }

    private static void Hook_StopSplash(Action orig) {
        EverestSplashHandler.SetSplashLoadingModCount((int)(totalToLoadField.GetValue(null) ?? 0) + extraMods);
        for (int i = 0; i < extraMods; i++) {
            Hook_SplashUpdate(EverestSplashHandler.IncreaseLoadedModCount, $"the loader to load the loading bar");
            Thread.Sleep(RNG.Next(1000));
        }
        orig();
    }

    private enum Mode {
        RandomGames,
        RandomStuff,
        Mad
    }
}