var map;
    $(document).ready(function(){
      map = new GMaps({
        el: '#adv_route_map',
        lat: -12.043333,
        lng: -77.028333
      });
      map.travelRoute({
        origin: [-12.044012922866312, -77.02470665341184],
        destination: [-12.090814532191756, -77.02271108990476],
        travelMode: 'driving',
        step: function(e){
          $('#instructions').append('<li>'+e.instructions+'</li>');
          $('#instructions li:eq('+e.step_number+')').delay(450*e.step_number).fadeIn(200, function(){
            map.drawPolyline({
              path: e.path,
              strokeColor: '#fb5d5d',
              strokeOpacity: 0.6,
              strokeWeight: 6
            });  
          });
        }
      });
    });