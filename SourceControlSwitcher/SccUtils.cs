// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Shell.Interop;
using System.Security.Principal;

namespace SourceControlSwitcher
{
    public partial class MainSite
    {
        public static readonly string[] AnkhSvnPackageIds = { "604ad610-5cf9-4bd5-8acc-f49810e2efd4" ,"321833BC-D00D-4076-9FDD-AC485EFDAEB2" };
        public const string AnkhSvnSccProviderId = "8770915b-b235-42ec-bbc6-8e93286e59b5";

        public static readonly string[] VisualSvnPackageIds = { "133240d5-fafa-4868-8fd7-5190a259e676", "83F1E506-04BC-4694-9C7D-C55B120E11F0" };
        public const string VisualSvnSccProviderId = "937cffd6-105a-4c00-a044-33ffb48a3b8f";

        public static readonly string[] VSToolsForGitPackageIds = { "7fe30a77-37f9-4cf2-83dd-96b207028e1b" };
        public const string VSToolsForGitSccProviderId = "11b8e6d7-c08b-4385-b321-321078cdd1f8";

        public static readonly string[] EasyGitIntegrationToolsPackageIds = { "c4128d99-2000-41d1-a6c3-704e6c1a3de2", "88d658b3-e361-4e7f-8f4d-9e78f6e4515a" };
        public const string EasyGitIntegrationToolsSccProviderId = "c4128d99-0000-41d1-a6c3-704e6c1a3de2";

        public static readonly string[] HgSccPackagePackageIds = { "a7f26ca1-2000-4729-896e-0bbe9e380635" };
        public const string HgSccPackageSccProviderId = "a7f26ca1-0000-4729-896e-0bbe9e380635";

        public static readonly string[] VisualHGPackageIds = { "dadada00-dfd3-4e42-a61c-499121e136f3" };
        public const string VisualHGSccProviderId = "dadada00-63c7-4363-b107-ad5d9d915d45";

        public static readonly string[] VSHGPackageIds = { "84a06d4f-da93-4015-a822-6b3d1b6d2756" };
        public const string VSHGProviderId = VisualHGSccProviderId;

        public static readonly string[] P4VSPackageIds =
        {
            "6c2c3283-424c-44b2-b1a7-b12d635de7cf", // P4VS 2025+
            "8d316614-311a-48f4-85f7-df7020f62357"  // Older P4VS
        };
        public const string P4VSProviderId = "fda934f4-0492-4f67-a6eb-cbe0953649f0";

        //TODO: at this point, just guessing about these ids, no success yet...
        //Questions: 
        //
        //  SccProviderId: What is this id, where does it come from, is it part of SourceControlSwitcher settings and thus
        //                  just needs to be a unique id?
        //
        //  Seems like the other ids need to be the extension guid registered with VS
        //  but where is this done, how can it be interrogated?
        //  
        //  Also, MS docs say this is a guid but the Vault extension the Id value is SOMETHING.guid
        //  Maybe when you install an extension, this first bit gets stripped off. Don't know yet.
        //
        //  Can unzip the extension installer and read the manifest file of the SourceGear Vault extensions
        //  Those values are below
        //
        //  Could it be that the entire Id string is used "VaultWitVsip.0da1b151-b5a1-4c82-b97d-bef83adf8958"
        //  That is not in line with SourceControlSwitcher's method of assuming this Id is a guid
        //  But this comes back to how to interrogate VS for the extension Id?

        //try the Vault Pro guids first - since Vault Pro is what I have installed
        //interesting that SourceGear has a prefix though - not in the MS docs that I can see
        //try just the guid bit as the SourceGearVaultSccProviderId
        //VaultWitVsip.0da1b151-b5a1-4c82-b97d-bef83adf8958
        //doesn't work, try using this guid as the package id

        //Vault Standard manifest has these two guids
        //Identity Id = "VaultSccVsip.bd27fdf0-45c3-4c8d-8fa5-aaf84600543c" Version="10.2.0.1222" Language="en-US" Publisher="SourceGear, LLC"
        //Dependency Version="[4.3.2.1,)" Location="VaultCxnSvcVsip\VaultCxnSvcVsip.vsix" DisplayName="SourceGear Vault Connection Service" Id="VaultCxnSvcVsip.4c27701c-5c7e-4c72-b316-790f176ceb76"

        //Vault Pro manifest has these two guids
        //Identity Id="VaultWitVsip.0da1b151-b5a1-4c82-b97d-bef83adf8958" Version="10.2.0.31222" Language="en-US" Publisher="SourceGear, LLC" 
        //Dependency Version="[4.3.2.1,)" Location="VaultSccVsix\VaultSccVsip.vsix" DisplayName="SourceGear Vault Professional Extension" Id="VaultSccVsip.1cbcff05-f0cd-4b07-ab12-1d986e26c8cf"

        //public static readonly string[] SourceGearVaultStandardPackageIds = { "03796505-8d85-4d99-8be3-860a7b046406" }; //made up guid
        public static readonly string[] SourceGearVaultStandardPackageIds = { "bd27fdf0-45c3-4c8d-8fa5-aaf84600543c" }; //SGStd VaultSccVsip
        //public static readonly string[] SourceGearVaultProfessionalPackageIds = { "1cbcff05-f0cd-4b07-ab12-1d986e26c8cf" }; //made up guid
        //public static readonly string[] SourceGearVaultProfessionalPackageIds = { "0da1b151-b5a1-4c82-b97d-bef83adf8958" }; //SGPro VaultWitVsip
        public static readonly string[] SourceGearVaultProfessionalPackageIds = { "1cbcff05-f0cd-4b07-ab12-1d986e26c8cf" }; //SGPro VaultSccVsip

        //TODO: Is this an internal guid for the Source Control Switcher...?
        //Seems like it must a guid that comes from the SourceGear Vault extension
        //TODO: made up guid, should it be??? probably not, failing on this now, or are we...?
        public const string SourceGearVaultSccProviderId = "f258d985-9463-41d3-ac34-531fe5f6e996";

        public const string SourceControlSwitcherCollection = "SourceControlSwitcher";

        public const string SubversionProviderProperty = "SubversionProvider";
        public const string GitProviderProperty = "GitProvider";
        public const string MercurialProviderProperty = "MercurialProvider";
        public const string PerforceProviderProperty = "PerforceProvider";
        public const string SourceGearVaultProviderProperty = "SourceGearVaultProvider";

        private static SccProvider GetCurrentSccProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetSccProviderFromGuid(GetCurrentSccProviderGuid());
        }

        private static SccProvider GetSccProviderFromGuid(string guid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            switch (guid)
            {
                case AnkhSvnSccProviderId:
                    return SccProvider.AnkhSvn;
                case VisualSvnSccProviderId:
                    return SccProvider.VisualSVN;
                case VSToolsForGitSccProviderId:
                    return SccProvider.VisualStudioToolsForGit;
                case EasyGitIntegrationToolsSccProviderId:
                    return SccProvider.EasyGitIntegrationTools;
                case HgSccPackageSccProviderId:
                    return SccProvider.HgSccPackage;
                case VisualHGSccProviderId:
                    return (IsSccPackageInstalled(VSHGPackageIds))
                        ? SccProvider.VSHG
                        : SccProvider.VisualHG;
                case P4VSProviderId:
                    return SccProvider.P4VS;
                case SourceGearVaultSccProviderId:
                    return SccProvider.SourceGearVault;
                default:
                    return SccProvider.Unknown;
            }
        }

        private static GitSccProvider GetGitSccProvider(string str)
        {
            var result = GitSccProvider.Default;
            Enum.TryParse(str, out result);
            return result;
        }

        private static SubversionSccProvider GetSubversionSccProvider(string str)
        {
            var result = SubversionSccProvider.Default;
            Enum.TryParse(str, out result);
            return result;
        }

        private static MercurialSccProvider GetMercurialSccProvider(string str)
        {
            var result = MercurialSccProvider.Default;
            Enum.TryParse(str, out result);
            return result;
        }
        private static PerforceSccProvider GetPerforceSccProvider(string str)
        {
            var result = PerforceSccProvider.Default;
            Enum.TryParse(str, out result);
            return result;
        }
        private static SourceGearVaultSccProvider GetSourceGearVaultSccProvider(string str)
        {
            var result = SourceGearVaultSccProvider.Default;
            Enum.TryParse(str, out result);
            return result;
        }
        private static RcsType GetRcsTypeFromSccProvider(SccProvider provider)
        {
            switch (provider)
            {
                case SccProvider.AnkhSvn:
                case SccProvider.VisualSVN:
                case SccProvider.VisualSVN_2019:
                    return RcsType.Subversion;
                case SccProvider.VisualStudioToolsForGit:
                case SccProvider.EasyGitIntegrationTools:
                case SccProvider.GitTools2019:
                    return RcsType.Git;
                case SccProvider.HgSccPackage:
                case SccProvider.VSHG:
                    return RcsType.Mercurial;
                case SccProvider.P4VS:
                    return RcsType.Perforce;
                case SccProvider.SourceGearVaultStandard:
                case SccProvider.SourceGearVaultProfessional:
                    return RcsType.SourceGearVault;
                default:
                    return RcsType.Unknown;
            }
        }

        public static void SetGitSccProvider(GitSccProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _SettingsStore.CreateCollection(SourceControlSwitcherCollection);
            if (provider == GitSccProvider.Default)
                _SettingsStore.DeleteProperty(SourceControlSwitcherCollection, GitProviderProperty);
            else
                _SettingsStore.SetString(SourceControlSwitcherCollection, GitProviderProperty, provider.ToString());

            if (provider == GitSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Git)
                RegisterPrimarySourceControlProvider(RcsType.Git);
        }

        public static void SetSubversionSccProvider(SubversionSccProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _SettingsStore.CreateCollection(SourceControlSwitcherCollection);
            if (provider == SubversionSccProvider.Default)
                _SettingsStore.DeleteProperty(SourceControlSwitcherCollection, SubversionProviderProperty);
            else
                _SettingsStore.SetString(SourceControlSwitcherCollection, SubversionProviderProperty, provider.ToString());

            if (provider == SubversionSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Subversion)
                RegisterPrimarySourceControlProvider(RcsType.Subversion);
        }

        public static void SetMercurialSccProvider(MercurialSccProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _SettingsStore.CreateCollection(SourceControlSwitcherCollection);
            if (provider == MercurialSccProvider.Default)
                _SettingsStore.DeleteProperty(SourceControlSwitcherCollection, MercurialProviderProperty);
            else
                _SettingsStore.SetString(SourceControlSwitcherCollection, MercurialProviderProperty, provider.ToString());

            if (provider == MercurialSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Mercurial)
                RegisterPrimarySourceControlProvider(RcsType.Mercurial);
        }

        public static void SetPerforceSccProvider(PerforceSccProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _SettingsStore.CreateCollection(SourceControlSwitcherCollection);
            if (provider == PerforceSccProvider.Default)
                _SettingsStore.DeleteProperty(SourceControlSwitcherCollection, PerforceProviderProperty);
            else
                _SettingsStore.SetString(SourceControlSwitcherCollection, PerforceProviderProperty, provider.ToString());

            if (provider == PerforceSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Perforce)
                RegisterPrimarySourceControlProvider(RcsType.Perforce);
        }

        public static void SetSourceGearVaultSccProvider(SourceGearVaultSccProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _SettingsStore.CreateCollection(SourceControlSwitcherCollection);
            if (provider == SourceGearVaultSccProvider.Default)
                _SettingsStore.DeleteProperty(SourceControlSwitcherCollection, SourceGearVaultProviderProperty);
            else
                _SettingsStore.SetString(SourceControlSwitcherCollection, SourceGearVaultProviderProperty, provider.ToString());

            if (provider == SourceGearVaultSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.SourceGearVault)
                RegisterPrimarySourceControlProvider(RcsType.SourceGearVault);
        }

        public static SubversionSccProvider GetSubversionSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SourceControlSwitcherCollection, SubversionProviderProperty, null);
            return GetSubversionSccProvider(providerStr);
        }

        public static MercurialSccProvider GetMercurialSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SourceControlSwitcherCollection, MercurialProviderProperty, null);
            return GetMercurialSccProvider(providerStr);
        }

        public static GitSccProvider GetGitSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SourceControlSwitcherCollection, GitProviderProperty, null);
            return GetGitSccProvider(providerStr);
        }

        public static SourceGearVaultSccProvider GetSourceGearVaultSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SourceControlSwitcherCollection, SourceGearVaultProviderProperty, null);
            return GetSourceGearVaultSccProvider(providerStr);
        }

        public static GitSccProvider GetDefaultGitSccProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsSccPackageInstalled(EasyGitIntegrationToolsPackageIds))
                return GitSccProvider.EasyGitIntegrationTools;

            if (IsSccPackageInstalled(VSToolsForGitPackageIds))
                return GitSccProvider.VisualStudioToolsForGit;

            return GitSccProvider.Disabled;
        }

        public static SubversionSccProvider GetDefaultSubversionSccProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsSccPackageInstalled(VisualSvnPackageIds))
                return SubversionSccProvider.VisualSVN;

            if (IsSccPackageInstalled(AnkhSvnPackageIds))
                return SubversionSccProvider.AnkhSVN;

            return SubversionSccProvider.Disabled;
        }

        public static MercurialSccProvider GetDefaultMercurialSccProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsSccPackageInstalled(VSHGPackageIds))
                return MercurialSccProvider.VSHG;

            if (IsSccPackageInstalled(VisualHGPackageIds))
                return MercurialSccProvider.VisualHG;

            if (IsSccPackageInstalled(HgSccPackagePackageIds))
                return MercurialSccProvider.HgSccPackage;

            return MercurialSccProvider.Disabled;
        }

        public static PerforceSccProvider GetDefaultPerforceSccProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsSccPackageInstalled(P4VSPackageIds))
                return PerforceSccProvider.P4VS;

            return PerforceSccProvider.Disabled;
        }

        public static SourceGearVaultSccProvider GetDefaultSourceGearVaultSccProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsSccPackageInstalled(SourceGearVaultStandardPackageIds))
                return SourceGearVaultSccProvider.SourceGearVaultStandard;

            if (IsSccPackageInstalled(SourceGearVaultProfessionalPackageIds))
                return SourceGearVaultSccProvider.SourceGearVaultProfessional;

            return SourceGearVaultSccProvider.Disabled;
        }

        public static bool IsSccPackageInstalled(string packageId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return IsSccPackageInstalled(Guid.Parse(packageId));
        }

        public static bool IsSccPackageInstalled(Guid packageId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int installed = 0;
            var hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            return installed == 1;
        }

        public static bool IsSccPackageInstalled(string[] packageId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetSccInstalledPackageId(packageId) != Guid.Empty;
        }

        public static bool IsSccPackageInstalled(Guid[] packageId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetSccInstalledPackageId(packageId) != Guid.Empty;
        }

        public static Guid GetSccInstalledPackageId(string[] packageIds)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var lst = new List<Guid>();
            foreach (var id in packageIds)
                lst.Add(new Guid(id));
            return GetSccInstalledPackageId(lst.ToArray());
        }

        public static Guid GetSccInstalledPackageId(Guid[] packageIds)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var g in packageIds)
                if (IsSccPackageInstalled(g)) return g;
            return Guid.Empty;
        }

        public static PerforceSccProvider GetPerforceSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SourceControlSwitcherCollection, PerforceProviderProperty, null);
            return GetPerforceSccProvider(providerStr);
        }

        public static RcsType GetLoadedRcsType()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SccProvider provider = GetCurrentSccProvider();
            return GetRcsTypeFromSccProvider(provider);
        }

        public static string GetCurrentSccProviderGuid()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Guid pGuid;
            _VsGetScciProviderInterface.GetSourceControlProviderID(out pGuid);
            return (pGuid == Guid.Empty) ? null : pGuid.ToString();
        }
    }

    public enum SccProvider
    {
        Unknown = 0,

        [Description("AnkhSVN")]
        [Display(Name = "AnkhSVN")]
        [LocDisplayName("AnkhSVN")]
        AnkhSvn,

        [Description("VisualSVN")]
        [Display(Name = "VisualSVN")]
        [LocDisplayName("VisualSVN")]
        VisualSVN,

        [Description("VisualSVN 2019")]
        [Display(Name = "VisualSVN 2019")]
        [LocDisplayName("VisualSVN 2019")]
        VisualSVN_2019,

        [Description("Git Source Control Provider")]
        [Display(Name = "Git Source Control Provider")]
        [LocDisplayName("Git Source Control Provider")]
        EasyGitIntegrationTools,

        [Description("EZ-GIT")]
        [Display(Name = "EZ-GIT")]
        [LocDisplayName("EZ-GIT")]
        GitTools2019,

        [Description("Visual Studio Tools for Git")]
        [Display(Name = "Visual Studio Tools for Git")]
        [LocDisplayName("Visual Studio Tools for Git")]
        VisualStudioToolsForGit,

        [Description("HgSccPackage")]
        [Display(Name = "HgSccPackage")]
        [LocDisplayName("HgSccPackage")]
        HgSccPackage,

        [Description("VisualHG")]
        [Display(Name = "VisualHG")]
        [LocDisplayName("VisualHG")]
        VisualHG,

        [Description("VSHG")]
        [Display(Name = "VSHG")]
        [LocDisplayName("VSHG")]
        VSHG,

        [Description("P4VS")]
        [Display(Name = "P4VS")]
        [LocDisplayName("P4VS")]
        P4VS,

        [Description("SourceGearVault")]
        [Display(Name = "SourceGear Vault")]
        [LocDisplayName("SourceGear Vault")]
        SourceGearVault,

        [Description("SourceGear Vault Standard")]
        [Display(Name = "SourceGear Vault Standard")]
        [LocDisplayName("SourceGear Vault Standard")]
        SourceGearVaultStandard,

        [Description("SourceGear Vault Professional")]
        [Display(Name = "SourceGear Vault Professional")]
        [LocDisplayName("SourceGear Vault Professional")]
        SourceGearVaultProfessional
    }

    public enum RcsType
    {
        Unknown = 0,
        Subversion,
        Git,
        Mercurial,
        Perforce,
        SourceGearVault
    }
}
