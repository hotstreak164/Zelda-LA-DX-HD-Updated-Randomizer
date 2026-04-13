using System;
using System.Collections.Generic;
using System.IO;

namespace LADXHD_Launcher;

public class AdvancedOption
{
    public string Key        { get; set; }
    public string RawValue   { get; set; }
    public string Tooltip    { get; set; }
    public bool   IsBool     => RawValue.Equals("true", StringComparison.OrdinalIgnoreCase) || RawValue.Equals("false", StringComparison.OrdinalIgnoreCase);
    public bool   IsFloat    => !IsBool && RawValue.Contains('.');
    public bool   IsInt      => !IsBool && !IsFloat;
    public bool   BoolValue  => RawValue.Equals("true", StringComparison.OrdinalIgnoreCase);
    public float  FloatValue => float.TryParse(RawValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f) ? f : 0f;
    public int    IntValue   => int.TryParse(RawValue, out int i) ? i : 0;

    public int DecimalPlaces
    {
        get
        {
            // Use the retrived value to determine the number of decimal places.
            if (!IsFloat) return 0;
            int dot = RawValue.IndexOf('.');
            if (dot < 0) return 2;
            return RawValue.Length - dot - 1;
        }
    }

    public string  FormatString => IsFloat ? "0." + new string('0', DecimalPlaces) : "0";
    public decimal Increment    => IsFloat ? (decimal)Math.Pow(10, -DecimalPlaces) : 1m;
}

public class AdvancedOptionGroup
{
    public string       ClassName      { get; set; } 
    public string       MainTooltip    { get; set; }  // tooltip without defaults line
    public List<string> DefaultValues  { get; set; } = new(); // split by "/"
    public List<AdvancedOption> Options { get; set; } = new();
    public bool AllowNegative => ClassName == "HUDOverlay";

    public string GetTooltipForIndex(int index)
    {
        string def = index < DefaultValues.Count
            ? $"\nDefault: {DefaultValues[index].Trim()}"
            : "";
        return string.IsNullOrEmpty(MainTooltip) ? "" : MainTooltip + def;
    }
}

public class AdvancedSection
{
    public string                    Header        { get; set; }
    public string                    HeaderTooltip { get; set; }
    public List<AdvancedOptionGroup> Groups        { get; set; } = new();
}

public static class AdvancedSettings
{
    public static readonly Dictionary<string, decimal> MinOverrides = new()
    {
        { "max_game_scale",           10     },
        { "classic_transition_speed", 0.01m  },
    };

    public static readonly Dictionary<string, decimal> MaxOverrides = new()
    {
        { "max_game_scale",           200   },
        { "classic_transition_speed", 5.00m },
        { "hearts_healed",            14    },
        { "light_bright",             1.00m },
        { "light_fade",               1.00m },
        { "bush_leaf_alpha",          1.00m },
        { "grass_leaf_alpha",         1.00m },
        { "chain_alpha",              1.00m },
        { "*_red",                    255   },  // matches anything ending in _red
        { "*_grn",                    255   },  // matches anything ending in _grn
        { "*_blu",                    255   },  // matches anything ending in _blu
        { "*_r",                      255   },  // matches tunic_grn_r etc
        { "*_g",                      255   },  // matches tunic_grn_g etc
        { "*_b",                      255   },  // matches tunic_grn_b etc
    };

    public static List<AdvancedSection> Sections { get; private set; } = new();

    // Stores current values keyed by "SectionHeader|Key"
    private static Dictionary<string, string> _values = new();

    private static string GetPath(string gameDirectory)
    {
        // If a "portable.txt" exists try to load from the game directory.
        string portable = Path.Combine(gameDirectory, "portable.txt");
        if (File.Exists(portable))
            return Path.Combine(gameDirectory, "advanced");

        // Check the "AppData\Local\Zelda_LA" path where save files are located.
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Zelda_LA", "advanced");
    }

    public static class OptionLabels
    {
        public static string Get(string key)
        {
            if (Labels.TryGetValue(key, out string label) && !string.IsNullOrEmpty(label))
                return label;
            return key;
        }

        private static readonly Dictionary<string, string> Labels = new()
        {
            // Game: Advanced Settings
            { "editor_mode",                    "Editor Mode"                       },
            { "max_game_scale",                 "Maximum Game Scale"                },

            // Game: User Interface
            { "menu_scale_override",            "Menus Scale Override"              },
            { "inventory_scale_override",       "Inventory Scale Override"          },
            { "textbox_scale",                  "Textbox Scale Override"            },

            // Game: HUD Overlay
            { "custom_items_show",              "Show Equipped Items"               },
            { "custom_items_scale",             "Equipped Items Scale"              },
            { "custom_items_offsetx",           "Equipped Items Offset X"           },
            { "custom_items_offsety",           "Equipped Items Offset Y"           },
            { "custom_heart_show",              "Show Hearts"                       },
            { "custom_heart_scale",             "Hearts Scale Override"             },
            { "custom_heart_offsetx",           "Hearts Offset X"                   },
            { "custom_heart_offsety",           "Hearts Offset Y"                   },
            { "custom_rupee_show",              "Show Rupees"                       },
            { "custom_rupee_scale",             "Rupees Scale Override"             },
            { "custom_rupee_offsetx",           "Rupees Offset X"                   },
            { "custom_rupee_offsety",           "Rupees Offset Y"                   },
            { "custom_keys_show",               "Show Small Keys"                   },
            { "custom_keys_scale",              "Small Keys Scale Override"         },
            { "custom_keys_offsetx",            "Small Keys Offset X"               },
            { "custom_keys_offsety",            "Small Keys Offset Y"               },
            { "custom_sicon_show",              "Show Save Icon"                    },
            { "custom_sicon_scale",             "Save Icon Scale"                   },

            // Camera: Advanced Settings
            { "classic_transition_speed",       "Classic Camera Transition Speed"   },

            // Player: Link
            { "swordbeam_level1",               "Level 1 Sword Beam"                },
            { "swordbeam_always",               "Always Sword Beam"                 },
            { "sword_charge_time",              "Sword Charge Time"                 },
            { "boots_charge_time",              "Boots Charge Time"                 },
            { "feather_velocity",               "Roc's Feather Jump Height"         },
            { "light_source",                   "Is Light Source"                   },
            { "light_red",                      "Light: Red Value"                  },
            { "light_grn",                      "Light: Green Value"                },
            { "light_blu",                      "Light: Blue Value"                 },
            { "light_bright",                   "Light: Brightness"                 },
            { "light_size",                     "Light: Radius"                     },
            { "dmg_shader_mark0",               "Damage Shader Mark 0"              },
            { "dmg_shader_mark1",               "Damage Shader Mark 1"              },
            { "dmg_shader_color1_red",          "Damage Shader Red C1"              },
            { "dmg_shader_color1_grn",          "Damage Shader Green C1"            },
            { "dmg_shader_color1_blu",          "Damage Shader Blue C1"             },
            { "dmg_shader_color2_red",          "Damage Shader Red C2"              },
            { "dmg_shader_color2_grn",          "Damage Shader Green C2"            },
            { "dmg_shader_color2_blu",          "Damage Shader Blue C2"             },
            { "dmg_shader_color3_red",          "Damage Shader Red C3"              },
            { "dmg_shader_color3_grn",          "Damage Shader Green C3"            },
            { "dmg_shader_color3_blu",          "Damage Shader Blue C3"             },

            // Items: Sword Shot
            { "swordbeam_damage",               "Sword Beam Damage"                 },
            { "swordbeam_duration",             "Sword Beam Duration"               },
            { "swordbeam_speed",                "Sword Beam Speed"                  },
            { "swordbeam_cast2d",               "Sword Beam Cast 2D"                },

            // Items: Arrows
            { "arrows_damage",                  "Arrows Damage"                     },
            { "arrows_distance",                "Arrows Distance"                   },
            { "arrows_speed",                   "Arrows Speed"                      },
            { "arrows_cast2d",                  "Arrows Cast 2D"                    },

            // Items: Bombs
            { "fuse_timer",                     "Bomb Fuse Timer"                   },
            { "arrow_pickup",                   "Arrows Always Pick-Up Bombs"       },
            { "item_interact",                  "Items Interact With Bombs"         },
            { "enemy_interact",                 "Interact With Enemy Bombs"         },
            { "fire_detonates",                 "Fire Detonates Bombs"              },

            // Items: Fairies
            { "sword_collect",                  "Collect With Sword"                },
            { "hearts_healed",                  "Number of Hearts Healed"           },

            // Items: Magic Powder
            { "powder_damage",                  "Magic Powder Damage"               },
            { "powder_gravity",                 "Magic Powder Gravity"              },

            // Items: Magic Rod
            { "magicrod_damage",                "Magic Rod Damage"                  },
            { "magicrod_duration",              "Magic Rod Shot Duration"           },
            { "magicrod_speed",                 "Magic Rod Shot Speed"              },
            { "magicrod_cast2d",                "Magic Rod Cast 2D"                 },

            // Graphics: Tunic Colors
            { "tunic_grn_r",                    "Tunic Green: Red Value"            },
            { "tunic_grn_g",                    "Tunic Green: Green Value"          },
            { "tunic_grn_b",                    "Tunic Green: Blue Value"           },
            { "tunic_blu_r",                    "Tunic Blue: Red Value"             },
            { "tunic_blu_g",                    "Tunic Blue: Green Value"           },
            { "tunic_blu_b",                    "Tunic Blue: Blue Value"            },
            { "tunic_red_r",                    "Tunic Red: Red Value"              },
            { "tunic_red_g",                    "Tunic Red: Green Value"            },
            { "tunic_red_b",                    "Tunic Red: Blue Value"             },

            // Graphics: Island Ocean Color
            { "ocean_color_red",                "Ocean Color: Red Value"            },
            { "ocean_color_grn",                "Ocean Color: Green Value"          },
            { "ocean_color_blu",                "Ocean Color: Blue Value"           },

            // Graphics: IntroScreen
            { "intro_sky_red",                  "Intro Raft: Sky Red"               },
            { "intro_sky_grn",                  "Intro Raft: Sky Green"             },
            { "intro_sky_blu",                  "Intro Raft: Sky Blue"              },
            { "intro_ocean_red",                "Intro Ocean: Center Red"           },
            { "intro_ocean_grn",                "Intro Ocean: Center Green"         },
            { "intro_ocean_blu",                "Intro Ocean: Center Blue"          },
            { "intro_ocean_lightning1_red",     "Intro Ocean: Lightning 1 Red"      },
            { "intro_ocean_lightning1_grn",     "Intro Ocean: Lightning 1 Green"    },
            { "intro_ocean_lightning1_blu",     "Intro Ocean: Lightning 1 Blue"     },
            { "intro_ocean_lightning2_red",     "Intro Ocean: Lightning 2 Red"      },
            { "intro_ocean_lightning2_grn",     "Intro Ocean: Lightning 2 Green"    },
            { "intro_ocean_lightning2_blu",     "Intro Ocean: Lightning 2 Blue"     },
            { "intro_ocean_bottom_red",         "Intro Ocean: Bottom Red"           },
            { "intro_ocean_bottom_grn",         "Intro Ocean: Bottom Green"         },
            { "intro_ocean_bottom_blu",         "Intro Ocean: Bottom Blue"          },
            { "intro_ocean_island_red",         "Intro Ocean Island: Red"           },
            { "intro_ocean_island_grn",         "Intro Ocean Island: Green"         },
            { "intro_ocean_island_blu",         "Intro Ocean Island: Blue"          },
            { "title_screen_sky_red",           "Title Screen: Sky Red"             },
            { "title_screen_sky_grn",           "Title Screen: Sky Green"           },
            { "title_screen_sky_blu",           "Title Screen: Sky Blue"            },

            // Objects: Bushes & Grass
            { "fall_to_ground",                 "Leaves Float to Ground"            },
            { "bush_leaf_alpha",                "Bush Leaves Alpha Value"           },
            { "grass_leaf_alpha",               "Grass Leaves Alpha Value"          },

            // Objects: BowWow's Chain
            { "chain_alpha",                    "BowWow Chain Alpha Value"          },

            // Objects: Crystals
            { "light_red_1",                    "Light 1: Red Value"                },
            { "light_grn_1",                    "Light 1: Green Value"              },
            { "light_blu_1",                    "Light 1: Blue Value"               },
            { "light_bright_1",                 "Light 1: Brightness"               },
            { "light_red_2",                    "Light 2: Red Value"                },
            { "light_grn_2",                    "Light 2: Green Value"              },
            { "light_blu_2",                    "Light 2: Blue Value"               },
            { "light_bright_2",                 "Light 2: Brightness"               },

            // Objects: Dungeon Teleporter
            { "enabled",                        "Enabled"                           },
            { "teleport_range",                 "Activate Range"                    },

            // Objects: Lamps
            { "powder_time",                    "Powder Light Duration"             },

            // Enemy: Flame Fountain
            { "sprite_shader",                  "Use Sprite Shader"                 },

            // Enemy: Lives
            { "AnglerFry",                      "Angler Fry"                        },
            { "ArmMimic",                       "Arm-Mimic"                         },
            { "AntiKirby",                      "Anti-Kirby"                        },
            { "AntiFairy",                      "Anti-Fairy"                        },
            { "BombiteGreen",                   "Green Bombite"                     },
            { "BonePutter",                     "Bone Putter"                       },
            { "BonePutterWing",                 "Winged Bone Putter"                },
            { "BooBuddy",                       "Boo Buddy"                         },
            { "BuzzBlob",                       "Buzz Blob"                         },
            { "CamoGoblin",                     "Camo Goblin"                       },
            { "CheepCheep",                     "Cheep-Cheep"                       },
            { "DarknutSpear",                   "Darknut (Spear)"                   },
            { "FlyingTile",                     "Flying Tile"                       },
            { "GhiniGiant",                     "Giant Ghini"                       },
            { "GopongaFlower",                  "Goponga Flower"                    },
            { "GopongaGiant",                   "Giant Goponga Flower"              },
            { "GreenZol",                       "Green Zol"                         },
            { "HardhatBeetle",                  "Hardhat Beetle"                    },
            { "IronMask",                       "Iron Mask"                         },
            { "LikeLike",                       "Like-Like"                         },
            { "MadBomber",                      "Mad Bomber"                        },
            { "MaskMimic",                      "Mask-Mimic"                        },
            { "MiniMoldorm",                    "Mini-Moldorm"                      },
            { "MoblinSword",                    "Moblin (Sword)"                    },
            { "MoblinPig",                      "Pig Moblin"                        },
            { "MoblinPigSword",                 "Pig Moblin (Sword)"                },
            { "OctorokWinged",                  "Winged Octorok"                    },
            { "PiranhaPlant",                   "Piranha Plant"                     },
            { "PokeyPart",                      "Pokey (Body Part)"                 },
            { "PolsVoice",                      "Pols Voice"                        },
            { "RedZol",                         "Red Zol"                           },
            { "RiverZora",                      "River Zora"                        },
            { "SeaUrchin",                      "Sea Urchin"                        },
            { "ShroudedStalfos",                "Shrouded Stalfos"                  },
            { "SpikedBeetle",                   "Spiked Beetle"                     },
            { "SpinyBeetle",                    "Spiny Beetle"                      },
            { "StalfosGreen",                   "Green Stalfos"                     },
            { "StalfosOrange",                  "Orange Stalfos"                    },
            { "StalfosKnight",                  "Stalfos Knight"                    },
            { "WaterTektite",                   "Water Tektite"                     },
            { "ArmosKnight",                    "Armos Knight"                      },
            { "BaCSoldier",                     "Ball & Chain Soldier"              },
            { "CueBall",                        "Cue Ball"                          },
            { "DesertLanmola",                  "Desert Lanmola"                    },
            { "DodongoSnake",                   "Dodongo Snake"                     },
            { "GiantBuzzBlob",                  "Giant Buzz Blob"                   },
            { "GrimCreeperFly",                 "Grim Creeper Bats"                 },
            { "KingMoblin",                     "King Moblin"                       },
            { "MStalfos",                       "Master Stalfos (First/Last)"       },
            { "MStalfosMid",                    "Master Stalfos (Middle)"           },
            { "RollingBones",                   "Rolling Bones"                     },
            { "StoneHinox",                     "Stone Hinox"                       },
            { "TurtleRock",                     "Turtle Rock"                       },
            { "AnglerFish",                     "Angler Fish"                       },
            { "EvilEagle",                      "Evil Eagle"                        },
            { "GenieBottle",                    "Genie Bottle"                      },
            { "HardHitBeetle",                  "HardHit Beetle"                    },
            { "HotHead",                        "Hot Head"                          },
            { "SlimeEel",                       "Slime Eel"                         },
            { "SlimeEye",                       "Slime Eye"                         },
            { "SlimeEyeHalf",                   "Slime Eye (Split)"                 },
            { "F_GiantZol",                     "Shadow Giant Zol"                  },
            { "F_Agahnim",                      "Shadow Agahnim"                    },
            { "F_Moldorm",                      "Shadow Moldorm"                    },
            { "F_Ganon",                        "Shadow Ganon"                      },
            { "F_DethI",                        "DethI"                             },
        };
    }

    public static int LoadMaxGameScale(string gameDirectory)
    {
        // If we can't find the "advanced" file default return 20.
        string path = GetPath(gameDirectory);
        if (!File.Exists(path)) return 20;

        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (!line.StartsWith("max_game_scale")) continue;

            int eq = line.IndexOf('=');
            if (eq < 0) continue;

            string val = line[(eq + 1)..].Trim();
            if (int.TryParse(val, out int result))
                return result;
        }
        return 20;
    }

    public static void Load(string gameDirectory)
    {
        string path = GetPath(gameDirectory);
        if (!File.Exists(path)) return;

        Sections.Clear();
        _values.Clear();

        var lines = File.ReadAllLines(path);

        AdvancedSection     currentSection    = null;
        bool                headerTooltipSet  = false;
        List<string>        tooltipLines      = new();
        List<string>        defaultValues     = new();
        AdvancedOptionGroup currentGroup      = null;
        string              _pendingClassName = null;

        foreach (string raw in lines)
        {
            string line = raw.Trim();

            // Skip the dividing bars.
            if (line.StartsWith("//---")) continue;

            // Create a new groupbox from the section header.
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentSection    = new AdvancedSection { Header = line[1..^1] };
                headerTooltipSet  = false;
                tooltipLines.Clear();
                defaultValues.Clear();
                currentGroup = null;
                Sections.Add(currentSection);
                continue;
            }
            if (currentSection == null) continue;

            // Class hint: skip these. It's a reference to the class it comes from.
            if (line.StartsWith("//:"))
            {
                _pendingClassName = line[3..].Trim();
                currentGroup = null;
                tooltipLines.Clear();
                defaultValues.Clear();
                continue;
            }

            // Skip normal comments.
            if (line.StartsWith("//"))
            {
                string comment = line[2..].Trim();

                // If there is comments before the option treat as tooltip for all options.
                if (!headerTooltipSet)
                {
                    currentSection.HeaderTooltip = comment;
                    headerTooltipSet = true;
                }

                // Strip out the default values for the tooltips.
                else if (comment.StartsWith("Default:") || comment.StartsWith("Defaults:"))
                {
                    int colon       = comment.IndexOf(':');
                    string valPart  = comment[(colon + 1)..].Trim();
                    defaultValues.Clear();
                    foreach (var d in valPart.Split('/', StringSplitOptions.TrimEntries))
                        defaultValues.Add(d);
                }
                // Add the line to the tooltip.
                else
                {
                    tooltipLines.Add(comment);
                }
                continue;
            }

            // The current line contains an option and value.
            if (line.Contains('='))
            {
                int eq     = line.IndexOf('=');
                string key = line[..eq].Trim();
                string val = line[(eq + 1)..].Trim();

                if (string.IsNullOrEmpty(key)) continue;

                // Create new group when we hit the first option after a //: block.
                if (currentGroup == null)
                {
                    currentGroup = new AdvancedOptionGroup
                    {
                        ClassName     = _pendingClassName,
                        MainTooltip   = string.Join(" ", tooltipLines).Trim(),
                        DefaultValues = new List<string>(defaultValues)
                    };
                    _pendingClassName = null;
                    currentSection.Groups.Add(currentGroup);
                    tooltipLines.Clear();
                    defaultValues.Clear();
                }

                // Create a new option to add to the menu.
                var option = new AdvancedOption
                {
                    Key      = key,
                    RawValue = val,
                    Tooltip  = currentGroup.GetTooltipForIndex(currentGroup.Options.Count)
                };

                // Add the option to the options group.
                currentGroup.Options.Add(option);
                _values[$"{currentSection.Header}|{key}"] = val;
            }
        }
    }

    public static void UpdateValue(string sectionHeader, string key, string value)
    {
        _values[$"{sectionHeader}|{key}"] = value;
    }

    public static void Save(string gameDirectory)
    {
        string path = GetPath(gameDirectory);
        if (!File.Exists(path)) return;

        var lines  = File.ReadAllLines(path);
        var output = new List<string>();

        string currentSection = null;

        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (line.StartsWith("[") && line.EndsWith("]"))
                currentSection = line[1..^1];

            if (line.Contains('=') && !line.StartsWith("//") && currentSection != null)
            {
                int eq     = line.IndexOf('=');
                string key = line[..eq].Trim();
                string vk  = $"{currentSection}|{key}";

                if (_values.TryGetValue(vk, out string newVal))
                {
                    int rawEq  = raw.IndexOf('=');
                    string lhs = raw[..rawEq];
                    output.Add($"{lhs}= {newVal}");
                    continue;
                }
            }

            output.Add(raw);
        }

        File.WriteAllLines(path, output);
    }
}