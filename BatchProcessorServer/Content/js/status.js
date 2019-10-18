var BatchUpdate = function() {
	
	this.dataPoints = [];
	this.timeInMs =Date.now();
	this.timeFormat = 'h:mm:ss';	
	this.dataPointsPayload = [];
	this.dataPointsQueue = [];
	this.dataPointsWorkers = [];
	this.dataPointsSlots = [];
	this.dataPointsCurrent = [];

	this.config = {
		type: 'line',
		data: {			
			datasets: [{
				label: 'Payloads',
				yAxisID: 'Low',
				fill: false,
				backgroundColor: 'black',
				borderColor: 'black',
				data: this.dataPointsPayload,
			},{
				label: 'Job Queue',
				yAxisID: 'High',
				fill: false,
				backgroundColor: 'blue',
				borderColor: 'blue',
				data: this.dataPointsQueue,
			},{
				label: 'Workers',
				yAxisID: 'Low',
				fill: false,
				backgroundColor: 'gray',
				borderColor: 'gray',
				data: this.dataPointsWorkers,
			},{
				label: 'Slots',
				yAxisID: 'High',
				fill: false,
				backgroundColor: 'green',
				borderColor: 'green',
				data: this.dataPointsSlots,
			},{
				label: 'Current Jobs',
				yAxisID: 'High',
				fill: false,
				backgroundColor: 'red',
				borderColor: 'red',
				data: this.dataPointsCurrent,
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
							quarter: this.timeFormat
						}
					},
					scaleLabel: {
						display:     false
					}
				}],
				yAxes: [{
					id: 'Low',
					type: 'linear',
					position: 'left',
					scaleLabel: {
						display:     true,
						labelString: 'Payloads / Workers'
					},
					ticks: {
						beginAtZero: true,
						suggestedMax: 5,
						maxTicksLimit: 5
					}
				}, {
					id: 'High',
					type: 'linear',
					position: 'right',
					scaleLabel: {
						display:     true,
						labelString: 'Jobs / Slots'
					},
					ticks: {
						beginAtZero: true,
						suggestedMax: 50,
						maxTicksLimit: 5
					}
				}]
			},
			elements: {
				point:{
					radius: 0
				}
			},
			responsiveAnimationDuration: 100,
			maintainAspectRatio: true,
			aspectRatio: 4
		}
	};

	this.ctx = $('#myChart');
	this.myLine = new Chart(this.ctx, this.config);
	
	// Helper method to update arrays to preserve animation
	this.updateDataArray = function(localData, serverData) {
	
		// Trim localData to preserve animations

		if (serverData.length > 0) {
			while (localData.length > 0 && localData[0].x < serverData[0].Key)			
				localData.shift();			
		} else {
			localData.length = 0;
		}
		
		// Append to localData to preserve animations

		$.each(serverData, function(key, value) {
			if (localData.length == 0 || localData[localData.length - 1].x < value.Key)
			{
				localData.push({
					x: value.Key,
					y: value.Value
				});
			}
		});

		// If we're somehow out of sync, just reset localData
		if (localData.length != serverData.length) {
			localData.length = 0;
			$.each(serverData, function(key, value) {			
				localData.push({
					x: value.Key,
					y: value.Value
				});
			});
		}
	}

	// Callback method from getJSON in updateStats
	this.updateStatsCallback = function(data) {
	
		// Update status

		$('#payloadCount').html(data.PayloadCount);
		$('#queueCount').html(data.QueueSize);
		$('#workerCount').html(data.TotalWorkers);
		$('#totalCount').html(data.TotalCount);
		$('#currentCount').html(data.TotalCurrent);
		$('#currentLoad').html(data.TotalLoadFormatted);

		// Update Chart
		
		this.updateDataArray(this.dataPointsPayload, data.ChartData.PayloadCount);
		this.updateDataArray(this.dataPointsQueue, data.ChartData.QueueSize);
		this.updateDataArray(this.dataPointsWorkers, data.ChartData.TotalWorkers);
		this.updateDataArray(this.dataPointsSlots, data.ChartData.TotalCount);
		this.updateDataArray(this.dataPointsCurrent, data.ChartData.TotalCurrent);

		if (data.ChartData.PayloadCount.length > 30)
			this.config.options.scales.xAxes[0].time.unit = 'minute';
		else
			this.config.options.scales.xAxes[0].time.unit = 'second';

		this.myLine.update();

		// Update workers

		var html = "";
		$.each(data.Workers, function(key, value) {
			html = html + '<div class="col-auto"><div class="outlined"><h5>' + value.Name + 
			'</h5>Load: ' + value.Current + ' / ' + value.Count + 
			'<br/><div class="progress"><div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" style="width: ' + value.LoadFormatted
			+ '" aria-valuenow="' + value.Current + '" aria-valuemin="0" aria-valuemax="' + value.Count + '"></div></div></div></div>';
		});

		$('#workers').html(html);

		setTimeout($.proxy(this.updateStats, this), 3000);
	}

	// Call once to perform repeated json queries to update page
	this.updateStats = function() {
		$.getJSON("update/status", $.proxy(this.updateStatsCallback, this));
	}
};

// Main entry point of script on window load
$(function () {
	window.batchData = new BatchUpdate();
	window.batchData.updateStats();
});