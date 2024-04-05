# Loading time optimizer
APRIL FOOLS WARNING
> This insanely complicated and complex mod has the sole goal to optimize the loading process of Everest to be up to 2147483647% faster. This means that Celeste will load so fast you may miss it if you blink your eyes for too long. As such, to be able to still enjoy the beautiful, gorgeous and magnificent splash screen (totally not biased) it is recommended you downgrade your PC specs.

> Be warned that this speedup may come at a cost, as such, certain old hardware may even burn because it is running too fast.
I AM NOT RESPONSIBLE FOR ANY DAMAGE TO YOUR HARDWARE THIS MOD MAY CAUSE. USE AT OWN RISK.

Happy April fools! (1/4/2024)

This mod actually just messes with the splash, making it either display funny names instead of the mod that is being loaded, or just messing with the progress bar.

This mod can be configured via the `everest-env.txt` file, sadly no in-game configs are available (yet).
In order to modify the following parameters you have to add the specified text as a new line to the aformentioned file, creating it if its non-existend (make sure its at your celeste's install root)

- `SPLASHFUNNY_SEED=N`: sets the seed for the random selection of modes and values to `N`
- `SPLASHFUNNY_MODE=N`: forces a mode to always be chosen, with the following table

| N | Behavior                           |
|---|------------------------------------|
| 0 | Display random game names          |
| 1 | Display random app/stuff names     |
| 2 | Make the progress bar move weirdly |

- `SPLASHFUNNY_DISABLEEXTRA`: when set to `1`, disables extra fake mods being added to the loading progress
- `SPLASHFUNNY_DISABLEMAD`: when set to `1`, disables the last mode in the previous table, keeping random selection of the other two enabled
Enjoy!
