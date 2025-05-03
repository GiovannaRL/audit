xPlanner.controller('ApplyTemplateCtrl', ['$scope', '$mdDialog', 'CostFieldList', 'WebApiService', 'toastr', 'local',
        'AuthService',
    function ($scope, $mdDialog, CostFieldList, WebApiService, toastr, local, AuthService) {

        var initialTemplate;

        $scope.link_disabled = false;
        $scope.costList = CostFieldList;
        $scope.data = {
            cost_field: CostFieldList[0].value
        };

        $scope.template_types = new Array;
        //TODO JULIANA: VERIFICAR COM A CAMILA QUAL A VAR QUE INDICA SE ESSE USUARIO TEM ACESSO AOS DADOS DA AUDAXWARE
        $scope.template_types.push({ id: 1, name: "Global Audaxware" });
        if (AuthService.getLoggedDomain() != 1) {
            $scope.template_types.push({ id: 2, name: "Global" });
        }
        $scope.template_types.push({ id: 3, name: "Project" });

        if (local.room.applied_id_template) {
            WebApiService.genericController.get({
                controller: 'templateRoom', action: 'ItemById',
                domain_id: local.room.domain_id,
                project_id: local.room.applied_id_template
            }, function (template) {

                initialTemplate = template;

                if (template.project_domain_id_template && template.project_id_template) {
                    $scope.template_type = 3;
                } else if (template.domain_id == 1) {
                    $scope.template_type = 1;
                } else {
                    $scope.template_type = 2;
                }
                $scope.data.link_template = $scope.data.delete_assets = local.room.linked_template;
            });
        }

        function GetParams(action, controller) {
            local.params.action = action;
            local.params.controller = controller;
            local.params.domain_id = AuthService.getLoggedDomain();
            return local.params
        };

        function getParams2(action, data, template_type) {
            return {
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                project_id: data.params.project_id,
                template_type: template_type
            };
        }

        WebApiService.genericController.query(GetParams("mis", "rooms"), function (rooms) {
            $scope.misRooms = rooms;
        });


        $scope.updateCheckbox = function () {
            $scope.data.delete_assets = true;
        }

        $scope.$watch('template_type', function (newValue) {
            if (newValue) {
                WebApiService.template_room_filtered.query(getParams2('TemplateList', local, newValue), function (templates) {
                    $scope.templates = templates;
                    $scope.templates.concat($scope.templates.map(function (item) {
                        item.name = item.department_type + ' - ' + item.description;
                    }));

                    if (initialTemplate) {
                        $scope.data.template = templates.find(function (t) { return t.template_id == initialTemplate.id });
                    }
                });
            }
        });

        $scope.$watch('data.template', function (newValue) {
            if (newValue) {
                var templateData = newValue;
                if (templateData.project_id_template) {
                    $scope.link_disabled = false;
                }
                else {
                    $scope.data.link_template = false;
                    $scope.link_disabled = true;
                }
            }
        });


        $scope.save = function () {

            $scope.applyTemplateForm.$setSubmitted();

            if ($scope.applyTemplateForm.$valid) {

                var dataSend = angular.copy($scope.data);
                dataSend.template_id = dataSend.template.template_id;
                dataSend.mis = dataSend.mis ? dataSend.mis : null;

                WebApiService.template_room.save(GetParams("Apply"), dataSend, function (data) {
                    toastr.success("Template Applied");
                    $mdDialog.hide();
                }, function (error) {
                    if (error.status == 409) {
                        toastr.info("Some, or all, of the equipment could not be deleted -- purchase orders have been issued!");
                    }
                    $mdDialog.cancel();
                });
            } else {
                $scope.applyTemplateForm.template.$setTouched();
                toastr.error("Please make sure you choose a template");
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);