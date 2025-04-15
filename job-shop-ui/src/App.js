import React, { useState } from 'react';
import './App.css';

function App() {
  // API call to fetch CSV files
  const [csvFiles, setCsvFiles] = useState([]);
  const [jobId, setJobId] = useState('');
  const [schedule, setSchedule] = useState([]);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const fetchCsvFiles = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/csv-files');
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
      const data = await response.json();
      setCsvFiles(data);
    } catch (error) {
      console.error('Error fetching CSV files:', error);
    }
  };
  // API call to fetch schedule based on selected CSV file
  const fetchSchedule = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`http://localhost:5000/api/schedule/${jobId}`);
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
      const data = await response.json();
      setSchedule(data);
    } catch (error) {
      setError('Error fetching schedule: ' + error.message);
    } finally {
      setLoading(false);
    }
  };
  // Fetch CSV files on component mount
  React.useEffect(() => {
    fetchCsvFiles();
  }, []);
  // Handle loading state
  if (loading) {
    return <div>Loading...</div>;
  }
  // Handle error state
  if (error) {
    return <div>Error: {error}</div>;
  }
  // Handle empty state

  if (csvFiles.length === 0) {
    return <div>No CSV files available. Please upload files to the server.</div>;
  }
  // Render the UI
  // Render the schedule
  return (
    <div className="App">
      <header className="App-header">
        <h1>Job Shop Scheduler</h1>
        <div>
          <select
            value={jobId}
            onChange={(e) => setJobId(e.target.value)}
          >
            <option value="">Select a Job File</option>
            {csvFiles.map((file, index) => (
              <option key={index} value={file}>
                {file}
              </option>
            ))}
          </select>
          <button onClick={fetchSchedule} disabled={!jobId}>
            Get Schedule
          </button>
        </div>
        <div>
          <h2>Schedule</h2>
          {schedule.length > 0 ? (
            <ul>
              {schedule.map((task, index) => (
                <li key={index}>
                  Job ID: {task.JobID}, Operation ID: {task.OperationID}, Subdivision: {task.Subdivision}, Start Time: {task.StartTime}, End Time: {task.EndTime}
                </li>
              ))}
            </ul>
          ) : (
            <p>No schedule available. Select a Job File to fetch the schedule.</p>
          )}
        </div>
      </header>
    </div>
  );
}

export default App;
