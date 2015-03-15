angular.module('beehive', ['ngWebSocket', 'ui.knob'])
    .factory('Buzz', function ($websocket, $timeout) {
        var dataStream = $websocket('ws://127.0.0.1:8336');
        var BUZZ_INTERVAL = 30;
        var delay = (function () {
            var timer = 0;
            return function (callback, ms) {
                clearTimeout(timer);
                timer = setTimeout(callback, ms);
            };
        })();
        var meter = {
            value: 0,
            options: {
                fgColor: '#FF0000',
                angleOffset: -125,
                angleArc: 250,
                max: 100,
                readOnly: true
            }
        };
        var max = 100;
        var buzzing = {};
        var Clean = function () {
            Object.keys(buzzing).forEach(function (user) {
                var now = new Date();
                if (buzzing[user] < now) delete buzzing[user];
            });
        };
        var Buzz = function (user) {
            var now = new Date();
            buzzing[user] = (now.setSeconds(now.getSeconds() + BUZZ_INTERVAL));
        };
        var Update = function () {
            Clean();
            $timeout(function () {
                meter.value = Object.keys(buzzing).length;
                meter.options.max = max;
            });
        }
        dataStream.onMessage(function (message) {
            var data = JSON.parse(message.data);
            if (data.Type == "buzz") {
                max = data.CurrentUsers;
                Buzz(data.User);
                Update();
                delay(Update, (BUZZ_INTERVAL * 1000));
            }
        });

        var model = {
            meter: meter
        };
        return model;
    })
    .controller('BuzzController', ['Buzz', '$scope', function (Buzz, $scope) {
        $scope.Buzz = Buzz;
    }]);