
pipeline {
    agent any
    stages {
        stage('clone repository') {
            steps {
                checkout scm
            }
        }
        stage('build images') {
            steps {
                script {
                    GIT_COMMIT_NUMBER = sh (
                        script: 'git rev-list HEAD --count --first-parent',
                        returnStdout: true
                    ).trim()
                }
                sh "docker build -t detibaholli/elefantivideobackend:latest -t detibaholli/elefantivideobackend:1.${GIT_COMMIT_NUMBER} ."
            }
        }

        stage('deploy images') {
            steps {
                sh "docker login -u detibaholli -p ${env['DOCKER-HUB-PASSWORD']}"
                sh 'docker push detibaholli/elefantivideobackend -a'
                sh 'docker logout'
            }
        }

        stage('deploy to kubernetes') {
            steps {
                withKubeConfig(caCertificate: '', clusterName: 'elefanti-video', contextName: 'elefanti-video', credentialsId: 'kube-config-file', namespace: 'default', serverUrl: 'https://elefanti-video-dns-af669090.hcp.westeurope.azmk8s.io:443') {
                    sh "kubectl set image deployment/video-backend-deployment elefanti-video-backend=detibaholli/elefantivideobackend:1.${GIT_COMMIT_NUMBER}"
                }
            }
        }
    }
}
