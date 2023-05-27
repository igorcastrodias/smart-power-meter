"use client";

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { Line } from "react-chartjs-2";
import resultsJson from "../../assets/results.json";
import "moment/locale/pt-br";
import moment from "moment";
import axios from "axios";
import { useEffect, useState } from "react";

type Result = {
  deviceId: number;
  day: string;
  totalConsumption: number;
};

type Data = {
  labels: string[];
  datasets: number[][];
};

moment.locale("pt-br");

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);
export const options = {
  responsive: true,
  plugins: {
    legend: {
      position: "top" as const,
    },
    title: {
      display: true,
      text: "Consumo por dispositivo",
    },
  },
};

export default function LineChart() {
  const [dados, setDados] = useState<Data>();

  async function getDados() {
    const response = await axios.get<Result[]>(
      "https://smartpowermeter-dev.azurewebsites.net/EnergyMeasurement"
    );
    setDados(getFilteredDataset(response.data));
  }

  useEffect(() => {
    getDados();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function getFilteredDataset(dataset: Result[]) {
    const labels: string[] = [];
    const datasets: number[][] = [[], []];

    for (let data of dataset) {
      if (labels.includes(data.day)) {
        continue;
      }
      labels.push(moment(data.day).format("DD-MM"));

      const otherIndex = data.deviceId === 1 ? 1 : 0;
      const actualIndex = data.deviceId === 1 ? 0 : 1;

      datasets[actualIndex].push(data.totalConsumption);
      const consumiptionOther = resultsJson.find(
        (x) => x.day === data.day && x.deviceId === otherIndex + 1
      );

      datasets[otherIndex].push(
        consumiptionOther ? consumiptionOther.totalConsumption : 0
      );
    }

    return { labels, datasets } as Data;
  }

  return (
    <Line
      options={options}
      data={{
        labels: dados?.labels || [],
        datasets: [
          {
            label: "Dispositivo 1",
            data: dados?.datasets[0] || [],
            borderColor: "rgb(255, 99, 132)",
            backgroundColor: "rgba(255, 99, 132, 0.5)",
          },
          {
            label: "Dispositivo 2",
            data: dados?.datasets[1] || [],
            borderColor: "rgb(53, 162, 235)",
            backgroundColor: "rgba(53, 162, 235, 0.5)",
          },
        ],
      }}
    />
  );
}
