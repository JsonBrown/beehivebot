angular.module('ui.knob', []).directive('knob', ['$timeout', function($timeout) {
    'use strict';

    return {
        restrict: 'EA',
        replace: true,
        template: '<input value="{{ knobData }}"/>',
        scope: {
            knobData: '=',
            knobOptions: '='
        },
        link: function($scope, $element) {
            var knobInit = $scope.knobOptions || {};

            knobInit.release = function(newValue) {
                $timeout(function() {
                    $scope.knobData = newValue;
                    $scope.$apply();
                });
            };

            $scope.$watch('knobData', function(newValue, oldValue) {
                if (newValue != oldValue) {
                    $($element).val(newValue).change();
                }
            });
            $scope.$watch('knobOptions', function (newValue, oldValue) {
                if (newValue != oldValue) {
                    $($element).trigger('configure', newValue);
                    $($element).trigger('change');
                }
            }, true);
            $($element).val($scope.knobData).knob(knobInit);
        }
    };
}]);