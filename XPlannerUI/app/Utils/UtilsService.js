xPlanner.factory('UtilsService', ['$filter', 'AssetClassList', 'AssetOptionSettings', 'WebApiService', 'DialogService', 'toastr', 'AuthService',
    function ($filter, AssetClassList, AssetOptionSettings, WebApiService, DialogService, toastr, AuthService) {

        function getJSNAttributesLabel() {

            var jsn_attributes = {
                jsn_code: 'JSN',
                jsn_nomenclature: 'JSN Nomenclature',
                jsn_description: 'JSN Description',
                jsn_comments: 'JSN Comments',
                jsn_utility1: 'U1',
                jsn_utility2: 'U2',
                jsn_utility3: 'U3',
                jsn_utility4: 'U4',
                jsn_utility5: 'U5',
                jsn_utility6: 'U6'
            }

            return jsn_attributes;
        };

        function getJSNUtilities() {
            return {
                u1: 'Utility 1: Plumbing (Water and Drainage)\nA | Hot and cold water\nB | Cold water and drain\nC | Hot water and drain\nD | Cold and hot water and drain\nE | Treated water and drain\nF | Cold, hot and treated water and drain\nG | Cold and treated water and drain\nH | Hot and treated water and drain\nI | Drain only\nJ | Cold water only',
                u2: 'Utility 2: Electrical\nA | 120 volt, conventional outlet\nB | 120 volt, special outlet\nC | 208/220 volt\nD | 120 and 208/220 volt\nE | 440 volt, 3 phase\nF | Special electrical requirements (includes, but is not limited to emergency power, multiple power connections, etc.)\nG | 208/220 volt, 3 phase',
                u3: 'Utility 3: Medical Gas (Provide operating pressures in accordance with NFPA 99)\nA | Oxygen\nB | Vacuum\nC | Air, low pressure (dental or non-medical)\nD | Air, high pressure (dental or non-medical)\nE | Oxygen and medical air\nH | Oxygen, vacuum and medical air\nJ | Vacuum and HP air\nK | Medical air',
                u4: 'Utility 4: Miscellaneous Gas\nA | Steam\nB | Nitrogen gas\nC | Nitrous oxide\nD | Nitrogen and nitrous oxide gas\nE | Carbon dioxide gas\nF | Liquid carbon dioxide\nG | Liquid nitrogen\nH | Instrument Air',
                u5: 'Utility 5: Non-Medical Gas\nA | Natural gas\nB | Liquid propane gas\nC | Methane\nD | Butane\nE | Propane\nF | Hydrogen gas\nG | Reserved\nH | Acetylene gas',
                u6: 'Utility 6: Miscellaneous\nA | Earth ground\nB | Lead lined walls\nC | Remote alarm ground\nD | Empty conduit with pull cord\nE | Vent to atmosphere\nF | Special gas requirements\nG | Lead lined walls; empty conduit with pull cord\nH | RF/Magnetic shielding\nJ | Wall/ceiling support required\nK | Empty conduit/pull cord & wall/ceiling support required\nL | Wall/ceiling support required; CAT 6 wire to nearest Telecommunications room\nM | Earth ground and wall/ceiling support required\nP | Lead lined walls and wall/ceiling support required\nT | CAT 6 wire to nearest Telecommunications Room'
            }
        };

        var _classes = {};
        AssetClassList.forEach(function (item) {
            _classes[item.id] = item.name;
        });

        function getAssetAttributesLabel(data, catalog) {

            var asset_attributes_type = {
                medgas_option: 'option',
                plumbing_option: 'option',
                water_option: 'option',
                data_option: 'option',
                network_option: 'option',
                electrical_option: 'option',
                blocking_option: 'option',
                supports_option: 'option',
                mobile_option: 'option',
                misc_seismic: 'checkbox',
                misc_ase: 'checkbox',
                misc_ada: 'checkbox',
                medgas_oxygen: 'checkbox',
                medgas_air: 'checkbox',
                medgas_n2o: 'checkbox',
                medgas_co2: 'checkbox',
                medgas_wag: 'checkbox',
                medgas_other: 'checkbox',
                medgas_nitrogen: 'checkbox',
                medgas_vacuum: 'checkbox',
                medgas_steam: 'checkbox',
                medgas_natgas: 'checkbox',
                misc_antimicrobial: 'checkbox',
                misc_ecolabel: 'checkbox',
                biomed_check_required: 'checkbox',
                plu_hot_water: 'checkbox',
                plu_cold_water: 'checkbox',
                plu_drain: 'checkbox',
                plu_return: 'checkbox',
                plu_treated_water: 'checkbox',
                plu_chilled_water: 'checkbox',
                plu_relief: 'checkbox',
                gas_liquid_co2: 'checkbox',
                gas_liquid_nitrogen: 'checkbox',
                gas_instrument_air: 'checkbox',
                gas_liquid_propane_gas: 'checkbox',
                gas_methane: 'checkbox',
                gas_butane: 'checkbox',
                gas_propane: 'checkbox',
                gas_hydrogen: 'checkbox',
                gas_acetylene: 'checkbox',
                misc_shielding_magnetic: 'checkbox',
                misc_shielding_lead_line: 'checkbox',
                medgas_high_pressure: 'checkbox'
            };

            if (Array.isArray(data)) {
                return data.map(function (i) {
                    for (var prop in i) {
                        switch (asset_attributes_type[prop]) {
                            case 'option':
                                i[prop] = (i[prop] == 1 ? 'Yes' : i[prop] == 2 ? 'Optional' : '--');
                                break;
                            case 'checkbox':
                                i[prop] = (i[prop] == 1 ? 'Yes' : '--');
                                break;
                        }
                    }
                    if (catalog) {
                        i.owner_name = $filter('capitalize')(i.owner_name);
                        i.id = i.domain_id.toString() + i.asset_id.toString();
                        i.comment = getCommentTemplate(i.comment);
                        i.cut_sheet = '<section align=center onclick="downloadFile(' + i.asset_id + ', ' + i.domain_id + ", 'fullcutsheet')\"><img class=\"icon\" src=\"" + (i.cut_sheet ? 'images/page_attach.png' : 'images/page.png') + '"></section>';
                        i.cad_block = i.cad_block ? "<section align=center onclick=\"downloadFile('" + i.cad_block + "', " + i.domain_id + ", 'cadblock')\"><img class=\"icon\" src=\"images/icons/autocad.ico\"></section>" : '';
                        i.revit = i.revit ? "<section align=center onclick=\"downloadFile('" + i.revit + "', " + i.domain_id + ", 'revit')\"><img class=\"icon\" src=\"images/icons/revit.ico\"></section>" : '';
                        i.jsn_description = i.jsn_description ? '<div align=center><i class="material-icons" title="' + i.jsn_description + '">comment</i></div>' : '';
                        i.jsn_comments = i.jsn_comments ? '<div align=center><i class="material-icons" title="' + i.jsn_comments + '">comment</i></div>' : '';
                    }
                    i.class = _classes[i.class] || '';
                    return i;
                });
            } else {
                return [];
            }

        };

        function getCommentTemplate(comment, keepOriginal) {
            if (keepOriginal)
                return { originalComment: comment, comment: comment ? "<div align=center><i class=\"material-icons\" title=\"" + comment + "\">comment</i></div>" : "" };

            return comment ? "<div align=center><i class=\"material-icons\" title=\"" + comment + "\">comment</i></div>" : "";
        };


        function getEmptyFileNoFirefox(filesArray) {
            return filesArray.find(function (item) {
                return !item.picture;
            });
        }

        function getEmptyFileFirefox(filesArray) {
            return filesArray.find(function (item, idx) {
                var elem = document.getElementById('photoFx' + idx);
                return !elem || !elem.files || elem.files.length <= 0;
            });
        }

        function isFirefox() {
            return navigator.userAgent.indexOf('Firefox') != -1;
        }

        function mapAssetOptionSetting(setting) {
            return AssetOptionSettings[setting];
        }

        function mapAssetOptionSettingReverse(setting) {
            for (var key in AssetOptionSettings) {
                if (AssetOptionSettings[key] === setting) {
                    return key;
                }
            }
        }

        function _showExpiratedPOs() {
            // Verifies if there are expirated POs and in the case it is true, displays the modal with the POs' information
            WebApiService.genericController.query({
                controller: "purchaseOrderExpirated", action: "All", domain_id: AuthService.getLoggedDomain()
            }, function (data) {
                if (data && data.length > 0) {
                    DialogService.openModal('app/POs/Modals/ExpiratedQuotePOs.html', 'ExpiratedQuotePOsCtr', { pos: data }, false);
                }
            }, function () {
                toastr.error('Error trying to retrieve expirated POs');
            });
        }

        return {
            GetJSNAttributesLabel: getJSNAttributesLabel,
            GetJSNUtilities: getJSNUtilities,
            GetAssetAttributesLabel: getAssetAttributesLabel,
            GetCommentTemplate: getCommentTemplate,
            GetEmptyFileNoFirefox: getEmptyFileNoFirefox,
            GetEmptyFileFirefox: getEmptyFileFirefox,
            IsFirefox: isFirefox,
            MapAssetOptionSetting: mapAssetOptionSetting,
            MapAssetOptionSettingReverse: mapAssetOptionSettingReverse,
            ShowExpiratedPOs: _showExpiratedPOs
        }
    }]);