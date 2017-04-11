package de.qaware.pn.service;

import com.microsoft.azure.sdk.iot.device.auth.Signature;
import com.microsoft.azure.sdk.iot.service.*;
import com.microsoft.azure.sdk.iot.service.devicetwin.DeviceMethod;
import com.microsoft.azure.sdk.iot.service.devicetwin.MethodResult;
import com.microsoft.azure.sdk.iot.service.exceptions.IotHubException;
import com.microsoft.azure.storage.StorageException;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.net.URLEncoder;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.time.Instant;
import java.time.temporal.ChronoUnit;
import java.util.concurrent.TimeUnit;

@Component
public class IotHubConnection {
    private static final String iotHubConnectionStringForRegistry = "HostName=SVH-Hub.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=svDdlDs6wb5x3upGXekO16sQYiY+1TpjaZvTYn/Pcoo=";
    private static final String iotHubConnectionStringForService = "HostName=SVH-Hub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=OJA4IUS+Vf7WPYqIwWDNLILXxBLJnPB0kf3WYDpT+dE=/Pcoo=";
    private static final Long responseTimeout = TimeUnit.SECONDS.toSeconds(200);
    private static final Long connectTimeout = TimeUnit.SECONDS.toSeconds(5);

    private final ServiceClient serviceClient;
    private final RegistryManager registryManager;

    public IotHubConnection() throws IOException, IotHubException, InterruptedException {
        serviceClient = ServiceClient.createFromConnectionString(
                iotHubConnectionStringForService, IotHubServiceClientProtocol.AMQPS);
        registryManager = RegistryManager.createFromConnectionString(iotHubConnectionStringForRegistry);
    }

    public void sendMessageToDevice(String deviceId) throws IOException, IotHubException, InterruptedException {
        if (serviceClient != null) {
            serviceClient.open();
            FeedbackReceiver feedbackReceiver = serviceClient
                    .getFeedbackReceiver(deviceId);
            if (feedbackReceiver == null) {
                throw new NullPointerException("Could not create FeedbackReceiver!");
            }
            feedbackReceiver.open();

            Message messageToSend = new Message("Cloud to device message.");
            messageToSend.setDeliveryAcknowledgement(DeliveryAcknowledgement.Full);

            serviceClient.send(deviceId, messageToSend);
            System.out.println("Message sent to device");

            FeedbackBatch feedbackBatch = feedbackReceiver.receive(10000);
            if (feedbackBatch != null) {
                System.out.println("Message feedback received, feedback time: "
                        + feedbackBatch.getEnqueuedTimeUtc().toString());
            }

            feedbackReceiver.close();
            serviceClient.close();
        }
    }

    public String getContainerSasToken(String deviceId) throws InvalidKeyException, StorageException {
        String token = null;
        try {
            String resourceUri = "SVH-Hub.azure-devices.net/devices/" + deviceId;
            resourceUri = URLEncoder.encode(resourceUri, "UTF-8");

            long expires = Instant.now().plus(1, ChronoUnit.MINUTES).getEpochSecond();

            Device device = registryManager.getDevice(deviceId);
            Signature signature = new Signature(resourceUri, expires, device.getPrimaryKey());

            token = String.format("SharedAccessSignature sr=%s&sig=%s&se=%s", resourceUri, signature.toString(), String.valueOf(expires));
        } catch (IotHubException | IOException iotf ) {
            iotf.printStackTrace();
        }

        return token;
    }

    public Object callMethod(String deviceId, String methodName, String payload) throws IOException {
        System.out.println("Starting sample...");
        DeviceMethod methodClient = DeviceMethod.createFromConnectionString(iotHubConnectionStringForRegistry);

        try
        {
            // Manage complete Method
            System.out.println("Getting device Method");
            MethodResult result = methodClient.invoke(deviceId, methodName, responseTimeout, connectTimeout, payload);
            if(result == null)
            {
                throw new IOException("Method invoke returns null");
            }
            System.out.println("Status=" + result.getStatus());
            System.out.println("Payload=" + result.getPayload());
            return result.getPayload();
        }
        catch (IotHubException e)
        {
            System.out.println(e.getMessage());
        }
        return null;
    }

    public Device registerDevice(String externalDeviceId) throws IOException, NoSuchAlgorithmException {
        Device device = Device.createFromId(externalDeviceId, null, null);
        try {
            device = registryManager.addDevice(device);
        } catch (IotHubException iote) {
            try {
                device = registryManager.getDevice(externalDeviceId);
            } catch (IotHubException iotf) {
                iotf.printStackTrace();
            }
        }
        System.out.println("Device ID: " + device.getDeviceId());
        System.out.println("Device key: " + device.getPrimaryKey());

        return device;
    }
}
