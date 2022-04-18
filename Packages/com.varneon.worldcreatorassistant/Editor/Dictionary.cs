using System;
using System.Collections.Generic;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class Dictionary : ScriptableObject
    {
        public List<Localization> Languages = new List<Localization>();

        [Serializable]
        public struct Localization
        {
            public string Language;
            public Translations Translations;
        }

        [Serializable]
        public struct Translations
        {
            public string ADD_NEW_CUSTOM_UNITYPACKAGE;
            public string ARE_YOU_SURE_CLEAR_VRC_SCRIPT_DEFINE_KEYWORDS;
            public string ARE_YOU_SURE_CLEAR_WCA_REGISTRY_KEYS;
            public string ASSET_IMPORTER;
            public string BROWSE;
            public string CANCEL;
            public string CHECK_FOR_UPDATES_GET_LATEST_VERSION;
            public string CHECK_FOR_UPDATES;
            public string CHOOSE_VRCSDK;
            public string CLEAN_INSTALL_INFO;
            public string CLEAN_INSTALL_VRCSDK_NOTICE;
            public string CLEAN_INSTALL_WARNING;
            public string CLEAN_INSTALL;
            public string CLEAR_VRC_SCRIPT_DEFINE_KEYWORDS;
            public string CLEAR_WCA_REGISTRY_KEYS;
            public string COMING_SOON;
            public string CONTINUE;
            public string COPY_LINK;
            public string COULD_NOT_FETCH_PACKAGE_LIST;
            public string CURRENT_FEATURES;
            public string DANGER_ZONE;
            public string DEFINE_VALID_CACHE_DIRECTORY;
            public string DEPRECATED;
            public string DETECTED_SDK_VARIANT;
            public string DO_YOU_WANT_CHECK_UPDATES_GITHUB;
            public string DO_YOU_WANT_CHECK_UPDATES_VRCSDK;
            public string DO_YOU_WANT_CHECK_UPDATES_WCA;
            public string DO_YOU_WANT_TO_UPDATE_VRCSDK;
            public string DOWNLOAD;
            public string DOWNLOADED;
            public string FEATURES_IN_DEVELOPMENT;
            public string FREQUENTLY_ASKED_QUESTIONS;
            public string GITHUB_API_NOT_RESPONDING;
            public string GITHUB_API_RATE_WARNING;
            public string GITHUB_COULD_NOT_FETCH_RELEASE;
            public string GITHUB_IMPORTER;
            public string GITHUB_NOT_ENOUGH_REQUESTS;
            public string IMPORT_ASSET_STORE_PACKAGES;
            public string IMPORT_COMMUNITY_TOOLS;
            public string IMPORT_OPEN_BETA_SDK_MANUALLY;
            public string IMPORT_UPM_PACKAGES;
            public string IMPORT;
            public string IMPORTED;
            public string IMPORTER;
            public string INSTALLED_VERSION;
            public string INVALID_PACKAGE_CACHE_DIRECTORY;
            public string LAST_CHECK;
            public string LAST_VRCAPI_REQUEST;
            public string LATEST_VERSION;
            public string MAIN;
            public string N_FILES_IN_INVALID_DIRECTORIES;
            public string N_FILES_HAVE_INVALID_GUID;
            public string NEXT;
            public string NO;
            public string NO_ASSET_STORE_PACKAGES_AVAILABLE;
            public string NOTE_VRC_LAYERS_AND_LIGHTMAP_GENERATION;
            public string OK;
            public string PACKAGE_CACHE_CANT_BE_INSIDE_PROJECT;
            public string PACKAGE_CACHE_DIRECTORY;
            public string PACKAGE_HAS_ALREADY_BEEN_ADDED;
            public string PACKAGE_IMPORTER;
            public string PACKAGE_WITH_SAME_PATH_ALREADY_ADDED;
            public string PLEASE_CHOOSE_VRCSDK;
            public string PLEASE_DEFINE_VALID_CACHE_DIRECTORY;
            public string PLEASE_WAIT_VRCAPI;
            public string PREFAB_REPOSITORIES;
            public string PREVIOUS;
            public string PROJECT_SETUP_OPTIONS;
            public string PSW_PAGE_HINT_CHOOSE_VRCSDK;
            public string PSW_PAGE_HINT_IMPORT_CUSTOM_UNITYPACKAGES;
            public string PSW_PAGE_HINT_SETUP_OPTIONS;
            public string PSW_PAGE_HINT_UPM_IMPORTER;
            public string PSW_PAGE_HINT_GITHUB_IMPORTER;
            public string PSW_PAGE_HINT_UAS_IMPORTER;
            public string PSW_REDUCE_REPOSITORIES_OR_WAIT;
            public string RECOMMENDED_COMMUNITY_TOOLS;
            public string REMOVE;
            public string RESET_UNUSED_LIGHTING_SETTINGS_DESC;
            public string RESET_UNUSED_LIGHTING_SETTINGS;
            public string RESETS;
            public string RESOURCES;
            public string SDK_VARIANT_NOT_DETECTED;
            public string SDK2_DEPRECATION_TEXT;
            public string SDK2_DESCRIPTION;
            public string SDK3_DESCRIPTION;
            public string SECONDS_AGO;
            public string SELECT_OPEN_BETA_SDK;
            public string SELECT_PACKAGE_CACHE_DIRECTORY;
            public string SELECT;
            public string SELECTED;
            public string SETTINGS;
            public string SETUP_MODE;
            public string SETUP_OPTIONS;
            public string SOME_COMMUNITY_TOOLS_UNAVAILABLE_SDK;
            public string SOME_WCA_FILES_INVALID_DIRECTORIES;
            public string SPECIFY_PACKAGE_CACHE_DIRECTORY_DESC;
            public string TUTORIALS;
            public string UNAVAILABLE;
            public string UNITY_ASSET_STORE;
            public string UNITYPACKAGE_IMPORTER;
            public string UP_TO_DATE;
            public string UPDATE_AVAILABLE;
            public string UPDATE;
            public string UPM_IMPORTER_DISABLED_TEMPORARILY;
            public string USEFUL_LINKS;
            public string USES_LEFT;
            public string VERSION_FILE_MISSING;
            public string VERSION_UNAVAILABLE;
            public string VRC_DOCUMENTATION;
            public string VRC_DOWNLOAD_PAGE;
            public string VRC_YOU_HAVE_ALREADY_CHECKED;
            public string WARNING;
            public string WCA_FEATURES_WIP;
            public string WCA_FEATURES;
            public string WCA_FOLDER_CLEANUP;
            public string WCA_IS_UP_TO_DATE;
            public string WCA_MAY_MALFUNCTION_AUTOMATIC_IMPORT;
            public string WCA_NEW_VERSION_AVAILABLE;
            public string WCA_THANK_YOU_FOR_USING;
            public string WCA_THIS_EDITOR;
            public string WCA_WILL_TRY_TO_CLEAR_DIRECTORIES;
            public string YES;
            public string YOU_ARE_ABOUT_TO_DOWNLOAD;
        }
    }
}
