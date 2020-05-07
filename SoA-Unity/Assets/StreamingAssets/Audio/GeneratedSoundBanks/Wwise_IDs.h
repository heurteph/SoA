/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_ARROSAGEAUTO = 3389613819U;
        static const AkUniqueID PLAY_HUMAIN_JOUE1 = 3185656221U;
        static const AkUniqueID PLAY_HUMAIN_JOUE2 = 3185656222U;
        static const AkUniqueID PLAY_HUMAIN_MANGE1 = 3446893022U;
        static const AkUniqueID PLAY_HUMAIN_MANGE2 = 3446893021U;
        static const AkUniqueID PLAY_HUMAIN_MANGEENFANT = 545734209U;
        static const AkUniqueID PLAY_HUMAIN_MANGEENFANT2 = 1978073185U;
        static const AkUniqueID PLAY_MOTEUR_ELAGUEUR = 2314452061U;
        static const AkUniqueID PLAY_MOTEUR_VOITUREELEC = 1403256204U;
        static const AkUniqueID PLAY_PARC_CHIEN = 4134052420U;
        static const AkUniqueID PLAY_PARC_EAU_LAC = 865874921U;
        static const AkUniqueID PLAY_PARC_INSECTES = 2727595513U;
        static const AkUniqueID PLAY_PARC_OISEAUX1 = 3725651506U;
        static const AkUniqueID PLAY_PARC_OISEAUX2 = 3725651505U;
        static const AkUniqueID PLAY_PARC_OISEAUX3 = 3725651504U;
        static const AkUniqueID PLAY_PARC_VENT = 4135899644U;
        static const AkUniqueID PLAY_PARC_VENT_01 = 3266099682U;
        static const AkUniqueID PLAY_PAS_JOGGUEUR = 2173732909U;
        static const AkUniqueID PLAY_PAS_JOGGUEUR2 = 596008677U;
        static const AkUniqueID PLAY_PLEURE_B_B_ = 4114146970U;
        static const AkUniqueID PLAY_RESPIRATION_JOGGUEURF = 1582928683U;
        static const AkUniqueID PLAY_RESPIRATION_JOGGUEURH = 1582928677U;
        static const AkUniqueID PLAY_RESPIRATIONMIX = 2793971418U;
        static const AkUniqueID PLAY_TEXTURE_PAS = 1047604054U;
        static const AkUniqueID PLAY_VETEMENT_ELAGUEUR = 2746796163U;
        static const AkUniqueID STOP_ARROSAGEAUTO = 1056565793U;
        static const AkUniqueID STOP_PARC_CHIEN = 2301453854U;
        static const AkUniqueID STOP_PARC_EAU_LAC = 3189001139U;
        static const AkUniqueID STOP_PARC_OISEAUX1 = 152275144U;
        static const AkUniqueID STOP_PARC_OISEAUX2 = 152275147U;
        static const AkUniqueID STOP_PARC_OISEAUX3 = 152275146U;
        static const AkUniqueID STOP_PARC_VENT = 2647100474U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace DANS_LIEU_REPOS
        {
            static const AkUniqueID GROUP = 3986250553U;

            namespace STATE
            {
                static const AkUniqueID NON = 544973834U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OUI = 645492566U;
            } // namespace STATE
        } // namespace DANS_LIEU_REPOS

    } // namespace STATES

    namespace SWITCHES
    {
        namespace COURT_MARCHE
        {
            static const AkUniqueID GROUP = 4156334155U;

            namespace SWITCH
            {
                static const AkUniqueID COURT = 2871560606U;
                static const AkUniqueID MARCHE = 1630799659U;
            } // namespace SWITCH
        } // namespace COURT_MARCHE

        namespace DROIT_GAUCHE
        {
            static const AkUniqueID GROUP = 3079589013U;

            namespace SWITCH
            {
                static const AkUniqueID DROIT = 2723061045U;
                static const AkUniqueID GAUCHE = 1211705900U;
            } // namespace SWITCH
        } // namespace DROIT_GAUCHE

        namespace PAS_MATIERE
        {
            static const AkUniqueID GROUP = 3869192313U;

            namespace SWITCH
            {
                static const AkUniqueID ASPHALT = 4169408098U;
                static const AkUniqueID BETON = 386080821U;
                static const AkUniqueID HERBE = 2495190089U;
                static const AkUniqueID TERRE = 508852877U;
            } // namespace SWITCH
        } // namespace PAS_MATIERE

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID PERSONNAGE_VITESSE = 3665353697U;
        static const AkUniqueID VITESSEVOITUREELEC = 428373397U;
        static const AkUniqueID VOLUMEECOUTEPERSO = 133973669U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID SOUNDBANKPC = 3163481603U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MIX_REVERB = 3296604463U;
        static const AkUniqueID NIVEAUECOUTEPERSO = 2734641465U;
        static const AkUniqueID PARC_ENVI_MASTER = 351745915U;
        static const AkUniqueID PARC_OBSTACLES_MASTER = 1797562303U;
        static const AkUniqueID PERSOMASTER = 386419066U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID MIXECOUTEPERSO = 2093609463U;
        static const AkUniqueID REVERB_LIEU_REPOS = 3204322453U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
