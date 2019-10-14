var dataPoints = [];
var timeInMs = Date.now();
var timeFormat = 'h:mm:ss';
var ctx = document.getElementById('myChart').getContext('2d');
var dataPointsPayload = [];
var dataPointsQueue = [];
var dataPointsWorkers = [];
var dataPointsSlots = [];
var dataPointsCurrent = [];

var config = {
	type: 'line',
	data: {			
		datasets: [{
			label: 'Payloads',
			yAxisID: 'Low',
			fill: false,
			backgroundColor: 'black',
			borderColor: 'black',
			data: dataPointsPayload,
		},{
			label: 'Job Queue',
			yAxisID: 'High',
			fill: false,
			backgroundColor: 'blue',
			borderColor: 'blue',
			data: dataPointsQueue,
		},{
			label: 'Workers',
			yAxisID: 'Low',
			fill: false,
			backgroundColor: 'gray',
			borderColor: 'gray',
			data: dataPointsWorkers,
		},{
			label: 'Slots',
			yAxisID: 'High',
			fill: false,
			backgroundColor: 'green',
			borderColor: 'green',
			data: dataPointsSlots,
		},{
			label: 'Current Jobs',
			yAxisID: 'High',
			fill: false,
			backgroundColor: 'red',
			borderColor: 'red',
			data: dataPointsCurrent,
		}]
	},
	options: {
		scales: {
			xAxes: [{
				type: 'time',
				time: {
					unit: 'second',
					distribution: 'linear',
					ticks: {
						source: 'auto'
					},
					displayFormats: {
						quarter: timeFormat
					}
				},
				scaleLabel: {
                    display:     true,
                    labelString: 'Time'
                }
			}],
			yAxes: [{
				id: 'Low',
				type: 'linear',
				position: 'left',
                scaleLabel: {
                    display:     true
                },
				ticks: {
					beginAtZero: true,
					suggestedMax: 5
				}
            }, {
				id: 'High',
				type: 'linear',
				position: 'right',
                scaleLabel: {
                    display:     true
                },
				ticks: {
					beginAtZero: true,
					suggestedMax: 50
				}
            }]
		},
		responsiveAnimationDuration: 100,
		maintainAspectRatio: true,
		aspectRatio: 5
	}
};

window.onload = function () {
	var ctx = document.getElementById("myChart").getContext("2d");
	window.myLine = new Chart(ctx, config);	
	updateStats();
};

function updateStats() {
    $.getJSON("update/status", function(data) {

		// Update status

		document.getElementById("payloadCount").innerHTML = data.PayloadCount;
		document.getElementById("queueCount").innerHTML = data.QueueSize;
		document.getElementById("workerCount").innerHTML = data.TotalWorkers;
		document.getElementById("totalCount").innerHTML = data.TotalCount;
		document.getElementById("currentCount").innerHTML = data.TotalCurrent;
		document.getElementById("currentLoad").innerHTML = data.TotalLoadFormatted;

		// Update Chart
		
		updateDataArray(dataPointsPayload, data.ChartData.PayloadCount);
		updateDataArray(dataPointsQueue, data.ChartData.QueueSize);
		updateDataArray(dataPointsWorkers, data.ChartData.TotalWorkers);
		updateDataArray(dataPointsSlots, data.ChartData.TotalCount);
		updateDataArray(dataPointsCurrent, data.ChartData.TotalCurrent);

		if (data.ChartData.PayloadCount.length > 30)
			config.options.scales.xAxes[0].time.unit = 'minute';
		else
			config.options.scales.xAxes[0].time.unit = 'second';

        window.myLine.update();

		// Update workers
		var html = "";
		$.each(data.Workers, function(key, value) {
			html = html + '<div class="col-auto"><div class="outlined"><h3>' + value.Name + 
			'</h3>Load: ' + value.Current + ' / ' + value.Count + 
			'<br/><div class="progress"><div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" style="width: ' + value.LoadFormatted
			+ '" aria-valuenow="' + value.Current + '" aria-valuemin="0" aria-valuemax="' + value.Count + '"></div></div></div></div>';
		});

		document.getElementById("workers").innerHTML = html;


        setTimeout(function(){updateStats()}, 3000);
    });
};

function updateDataArray(localData, serverData) {
	if (serverData.length > 0)
	{
		while (localData.length > 0 && localData[0].x < serverData[0].Key)			
			localData.shift();			
	}
	else
	{
		localData.length = 0;
	}
		
    $.each(serverData, function(key, value) {
		if (localData.length == 0 || localData[localData.length - 1].x < value.Key)
		{
			localData.push({
				x: value.Key,
				y: value.Value
			});
		}
    });
}