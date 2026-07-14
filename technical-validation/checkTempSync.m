%% checksync.m
% Arduino sync test: compares EEG trigger channel timestamps vs Unity LSL marker timestamps
% Requires the xdf-matlab module (load_xdf.m) in the same folder

clear; clc; close all;

%% 1. Load XDF
addpath(fullfile('\path\to\base\folder\'));
xdf_file = '\path\to\xdf\';

fprintf('Loading XDF file...\n');
[streams, fileheader] = load_xdf(xdf_file);

%% 2. Inspect available streams
fprintf('\n--- Available streams ---\n');
for i = 1:length(streams)
    name = streams{i}.info.name;
    stype = streams{i}.info.type;
    srate = streams{i}.info.nominal_srate;
    nsamples = length(streams{i}.time_stamps);
    fprintf('Stream %d: name="%s"  type="%s"  srate=%s  nsamples=%d\n', ...
        i, name, stype, srate, nsamples);
end

%% 3. Identify streams manually based on printed info above
unity_stream_idx   = 4;  % ArduinoPulseMarkers
bv_marker_stream_idx = 1;  % BrainVision RDA Markers

%% 4. Extract BrainVision trigger onset times
bv_markers    = streams{bv_marker_stream_idx}.time_series;
bv_timestamps = streams{bv_marker_stream_idx}.time_stamps;

% Convert once
bv_markers_str = string(bv_markers);

% Unique markers
unique_markers = unique(bv_markers_str);
fprintf('\nUnique BrainVision marker types:\n');
disp(unique_markers);

% Filter onsets
onset_idx = bv_markers_str == 'S239';
eeg_pulse_times = bv_timestamps(onset_idx);
fprintf('Found %d onset triggers in BrainVision marker stream\n', length(eeg_pulse_times));

%% 5. Extract Unity marker timestamps
unity_timestamps = streams{unity_stream_idx}.time_stamps;
fprintf('Found %d PULSE markers in Unity stream\n', length(unity_timestamps));
unity_pulse_times = unity_timestamps;

%% 6. Match pulses and compute delays
n_pulses = min(length(eeg_pulse_times), length(unity_pulse_times));

if n_pulses == 0
    error('No matching pulses found. Check stream indices and trigger channel.');
end

delays_ms = (unity_pulse_times(1:n_pulses) - eeg_pulse_times(1:n_pulses)) * 1000;

fprintf('\n--- Sync Results (%d pulses) ---\n', n_pulses);
fprintf('Mean delay (Unity - EEG): %.2f ms\n',  mean(delays_ms));
fprintf('Std deviation:            %.2f ms\n',  std(delays_ms));
fprintf('Min delay:                %.2f ms\n',  min(delays_ms));
fprintf('Max delay:                %.2f ms\n',  max(delays_ms));
fprintf('Max jitter (range):       %.2f ms\n',  max(delays_ms) - min(delays_ms));

%% 7. Plot
figure('Name', 'Arduino Sync Test', 'Color', 'w');

subplot(2,1,1);
plot(delays_ms, 'b.-', 'MarkerSize', 10);
yline(mean(delays_ms), 'r--', sprintf('Mean = %.2f ms', mean(delays_ms)), 'LineWidth', 1.5);
xlabel('Pulse number');
ylabel('Delay (ms)');
title('Unity marker timestamp - EEG trigger timestamp per pulse');
grid on;

subplot(2,1,2);
histogram(delays_ms, 20, 'FaceColor', 'b', 'EdgeColor', 'w');
xlabel('Delay (ms)');
ylabel('Count');
title(sprintf('Delay distribution  |  mean=%.2f ms  std=%.2f ms', mean(delays_ms), std(delays_ms)));
grid on;

saveas(gcf, '\path\to\png\sync_result.png');
fprintf('\nPlot saved as sync_result.png\n');

%% 8. Check jitter in real Task data (Stream 3, 100Hz)
task_stream_idx = 3;
task_timestamps = streams{task_stream_idx}.time_stamps;
task_name       = streams{task_stream_idx}.info.name;

fprintf('\n--- Task Stream Jitter Analysis ---\n');
fprintf('Stream: "%s"\n', task_name);
fprintf('Nominal sample rate: %s Hz\n', streams{task_stream_idx}.info.nominal_srate);
fprintf('N samples: %d\n', length(task_timestamps));

% Compute inter-sample intervals
task_intervals_ms = diff(task_timestamps) * 1000;
expected_interval = 1000 / 100;  % 10ms at 100Hz

fprintf('Expected interval:  %.2f ms\n', expected_interval);
fprintf('Mean interval:      %.2f ms\n', mean(task_intervals_ms));
fprintf('Std deviation:      %.2f ms\n', std(task_intervals_ms));
fprintf('Min interval:       %.2f ms\n', min(task_intervals_ms));
fprintf('Max interval:       %.2f ms\n', max(task_intervals_ms));
fprintf('Jitter range:       %.2f ms\n', max(task_intervals_ms) - min(task_intervals_ms));

% Plot
figure('Name', 'Task Stream Jitter', 'Color', 'w');

subplot(3,1,1);
plot(task_intervals_ms, 'b-', 'LineWidth', 0.5);
yline(expected_interval, 'r--', sprintf('Expected = %.1f ms', expected_interval), 'LineWidth', 1.5);
yline(mean(task_intervals_ms), 'g--', sprintf('Mean = %.2f ms', mean(task_intervals_ms)), 'LineWidth', 1.5);
xlabel('Sample number');
ylabel('Interval (ms)');
title(sprintf('Inter-sample intervals: %s (nominal 100Hz)', task_name));
grid on;

subplot(3,1,2);
histogram(task_intervals_ms, 50, 'FaceColor', 'b', 'EdgeColor', 'w');
xlabel('Interval (ms)');
ylabel('Count');
title(sprintf('Interval distribution  |  mean=%.2f ms  std=%.2f ms', ...
    mean(task_intervals_ms), std(task_intervals_ms)));
grid on;

subplot(3,1,3);
% Zoom in on first 500 samples to see pattern clearly
n_show = min(500, length(task_intervals_ms));
plot(task_intervals_ms(1:n_show), 'b.-', 'MarkerSize', 4);
yline(expected_interval, 'r--', 'LineWidth', 1.5);
xlabel('Sample number (first 500)');
ylabel('Interval (ms)');
title('First 500 intervals — checking for zigzag game loop pattern');
grid on;

saveas(gcf, '\path\to\png\task_jitter.png');
fprintf('\nTask jitter plot saved to task_jitter.png\n');